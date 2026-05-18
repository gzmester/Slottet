using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Domain.Enums;
using Application.DTOs.Authorization;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<Employee> _userManager;
    private readonly SignInManager<Employee> _signInManager;
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _db;

    public AuthController(
        UserManager<Employee> userManager,
        SignInManager<Employee> signInManager,
        IConfiguration config,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _db = db;
    }

    /// <summary>
    /// Login med email + pincode.
    /// Hvis brugeren ikke har en pincode endnu, returneres requiresSetup = true.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        // 1. Find employee by email
        var user = await _userManager.Users
            .Include(u => u.Authorization)
            .Include(u => u.Roles)
            .Include(u => u.Shifts)
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null)
        {
            await LogFailedLogin(model.Email);
            return Unauthorized(new { message = "Forkert email eller pinkode." });
        }

        // 2. Check if user has set up their pincode yet
        if (!user.HasPincode)
        {
            return Ok(new AuthResponseDto
            {
                RequiresSetup = true,
                EmployeeId = user.Id,
                EmployeeName = $"{user.FirstName} {user.LastName}".Trim(),
                Message = "Opret en pinkode for at fortsætte."
            });
        }

        // 3. Verify pincode using Identity's password verification
        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Pincode, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                await LogFailedLogin(model.Email, "Konto låst - for mange fejlede forsøg");
                return Unauthorized(new { message = "Kontoen er låst pga. for mange fejlede forsøg." });
            }

            await LogFailedLogin(model.Email);
            return Unauthorized(new { message = "Forkert email eller pinkode." });
        }

        // 4. Get Identity roles (Admin, Vagtansvarlig, Plejepersonale)
        var identityRoles = await _userManager.GetRolesAsync(user);

        // 5. Determine current shift type based on today's shift
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayShift = user.Shifts
            .FirstOrDefault(s => s.Date == today);
        var currentShiftType = todayShift?.ShiftType.ToString() ?? "Ingen vagt i dag";

        // 6. Generate JWT token
        var token = GenerateJwtToken(user, identityRoles, todayShift);
        var employeeName = $"{user.FirstName} {user.LastName}".Trim();

        // 7. Log successful login
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType = "Access",
            Action = "Bruger logget ind",
            Entity = "Auth",
            EntityId = user.Id.ToString(),
            UserId = user.Id,
            UserName = employeeName
        });
        await _db.SaveChangesAsync();

        // 8. Return comprehensive response
        return Ok(new AuthResponseDto
        {
            Token = token,
            EmployeeName = employeeName,
            EmployeeId = user.Id,
            AuthorizationRole = user.Authorization?.Role.ToString() ?? string.Empty,
            IdentityRoles = identityRoles.ToList(),
            CurrentShiftType = currentShiftType,
            LocationID = user.LocationID
        });
    }

    /// <summary>
    /// Første-gangs opsætning: Opret pinkode for en medarbejder der ikke har en endnu.
    /// </summary>
    [HttpPost("setup-pincode")]
    public async Task<IActionResult> SetupPincode([FromBody] SetupPincodeModel model)
    {
        var user = await _userManager.FindByIdAsync(model.EmployeeId.ToString());
        if (user == null)
            return NotFound(new { message = "Medarbejder findes ikke." });

        if (user.HasPincode)
            return BadRequest(new { message = "Denne medarbejder har allerede en pinkode." });

        // Set UserName if missing (Identity requires it)
        if (string.IsNullOrEmpty(user.UserName))
        {
            user.UserName = user.Email;
        }

        // Set the pincode as Identity password (hashed)
        var result = await _userManager.AddPasswordAsync(user, model.Pincode);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Auto-assign Identity role based on Authorization table
        var userWithAuth = await _userManager.Users
            .Include(u => u.Authorization)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (userWithAuth?.Authorization != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count == 0)
            {
                var identityRole = userWithAuth.Authorization.Role switch
                {
                    AuthorizationRole.Admin => "Admin",
                    AuthorizationRole.Scheduler => "Vagtansvarlig",
                    _ => "Plejepersonale"
                };
                await _userManager.AddToRoleAsync(user, identityRole);
            }
        }

        // Log pincode setup
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType = "Access",
            Action = "Pinkode oprettet",
            Entity = "Auth",
            EntityId = user.Id.ToString(),
            UserId = user.Id,
            UserName = $"{user.FirstName} {user.LastName}"
        });
        await _db.SaveChangesAsync();

        return Ok(new { message = "Pinkode oprettet succesfuldt." });
    }

    /// <summary>
    /// Tildel Identity-rolle til en medarbejder (kun Admin).
    /// </summary>
    [HttpPost("assign-role")]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleModel model)
    {
        var user = await _userManager.FindByIdAsync(model.EmployeeId.ToString());
        if (user == null)
            return NotFound(new { message = "Medarbejder findes ikke." });

        // Validate role is one of the allowed values
        var validRoles = new[] { "Admin", "Vagtansvarlig", "Plejepersonale" };
        if (!validRoles.Contains(model.Role))
            return BadRequest(new { message = $"Ugyldig rolle. Gyldige roller: {string.Join(", ", validRoles)}" });

        // Remove existing Identity roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Add the new role
        var addResult = await _userManager.AddToRoleAsync(user, model.Role);
        if (!addResult.Succeeded)
            return BadRequest(addResult.Errors);

        // Log role assignment
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType = "Access",
            Action = $"Rolle ændret til {model.Role}",
            Entity = "Auth",
            EntityId = user.Id.ToString(),
            UserId = user.Id,
            UserName = $"{user.FirstName} {user.LastName}"
        });
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Rolle '{model.Role}' tildelt til {user.FirstName} {user.LastName}." });
    }

    private string GenerateJwtToken(Employee user, IList<string> identityRoles, Shift? currentShift)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim("EmployeeId", user.Id.ToString()),
            new Claim("AuthorizationRole", user.Authorization?.Role.ToString() ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add Identity role claims (Admin, Vagtansvarlig, Plejepersonale)
        foreach (var role in identityRoles)
        {
            claimsList.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add current shift type as claim
        if (currentShift != null)
        {
            claimsList.Add(new Claim("ShiftType", currentShift.ShiftType.ToString()));
        }

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claimsList,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task LogFailedLogin(string email, string? action = null)
    {
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType = "Access",
            Action = action ?? "Login forsøg fejlet",
            Entity = "Auth",
            EntityId = "unknown",
            UserId = null,
            UserName = email
        });
        await _db.SaveChangesAsync();
    }
}
