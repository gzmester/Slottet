using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Application.DTOs.Authorization;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Employee> _userManager;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;

        public AuthController(UserManager<Employee> userManager, IConfiguration config, ApplicationDbContext db)
        {
            _userManager = userManager;
            _config = config;
            _db = db;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // 1. Search for the employee using the pincode from the database and include roles
            var user = await _userManager.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.PinCode == model.Pincode);

            // 2. If user exists with matching pincode, generate JWT token
            if (user != null)
            {
                var token = GenerateJwtToken(user);
                var employeeName = $"{user.FirstName} {user.LastName}".Trim();

                // Log succesfuldt login
                await _db.AuditLogs.AddAsync(new AuditLog
                {
                    LogType  = "Access",
                    Action   = "LoggedIn",
                    Entity   = "Auth",
                    EntityId = user.Id.ToString(),
                    UserId   = user.Id,
                    UserName = employeeName
                });
                await _db.SaveChangesAsync();

                return Ok(new AuthResponseDto { Token = token, EmployeeName = employeeName });
            }

            // Log fejlet login
            await _db.AuditLogs.AddAsync(new AuditLog
            {
                LogType  = "Access",
                Action   = "FailedLogin",
                Entity   = "Auth",
                EntityId = "unknown",
                UserId   = null,
                UserName = "unknown"
            });
            await _db.SaveChangesAsync();

            return Unauthorized(new { message = "Invalid Pincode." });
        }

        private string GenerateJwtToken(Employee user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Build claims list with roles
            var claimsList = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim("EmployeeId", user.Id.ToString())
            };

            // Add role claims for each role the employee has
            if (user.Roles != null && user.Roles.Any())
            {
                foreach (var role in user.Roles)
                {
                    claimsList.Add(new Claim(ClaimTypes.Role, role.Name ?? string.Empty));
                }
            }

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claimsList,
                expires: DateTime.UtcNow.AddHours(12), // Longer session for internal app
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
