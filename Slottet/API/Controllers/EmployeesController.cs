using Application.DTOs.Employee;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly UserManager<Employee> _userManager;
    private readonly ApplicationDbContext _db;

    public EmployeesController(UserManager<Employee> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    // Helper: Current user ID from JWT
    private int CurrentUserId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    private bool IsAdmin => User.IsInRole("Admin");

    // GET /api/employees
    [HttpGet]
    [Authorize(Policy = "RequireScheduler")]
    public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetAll()
    {
        var employees = await _userManager.Users
            .Include(e => e.Location)
            .Include(e => e.Roles)
            .ToListAsync();

        var result = new List<EmployeeResponseDto>();
        foreach (var e in employees)
        {
            var identityRoles = await _userManager.GetRolesAsync(e);
            result.Add(MapToResponseDto(e, identityRoles.ToList()));
        }
        return Ok(result);
    }

    // GET /api/employees/{id}
    [HttpGet("{id}")]
    [Authorize(Policy = "RequireCareStaff")]
    public async Task<ActionResult<EmployeeResponseDto>> GetById(int id)
    {
        // Plejepersonale kan kun se sig selv, Admin/Vagtansvarlig kan se alle
        if (!IsAdmin && !User.IsInRole("Vagtansvarlig") && id != CurrentUserId)
            return Forbid();

        var employee = await _userManager.Users
            .Include(e => e.Location)
            .Include(e => e.Roles)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
            return NotFound();

        var identityRoles = await _userManager.GetRolesAsync(employee);
        return Ok(MapToResponseDto(employee, identityRoles.ToList()));
    }

    // POST /api/employees
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<EmployeeResponseDto>> Create(EmployeeCreateDto dto)
    {
        var locationExists = await _db.Locations.AnyAsync(l => l.LocationID == dto.LocationID);
        if (!locationExists)
            return BadRequest($"Location med ID {dto.LocationID} findes ikke.");

        // Valider at Identity-rolle er gyldig
        var validRoles = new[] { "Admin", "Vagtansvarlig", "Plejepersonale" };
        if (!validRoles.Contains(dto.Role))
            return BadRequest($"Ugyldig rolle. Gyldige: {string.Join(", ", validRoles)}");

        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            LocationID = dto.LocationID,
            AuthorizationID = 1 // Default — ikke længere brugt til adgangskontrol
        };

        // Opret UDEN password — ansat opretter pinkode ved første login
        var result = await _userManager.CreateAsync(employee);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Tildel Identity-rolle direkte
        await _userManager.AddToRoleAsync(employee, dto.Role);

        // Log oprettelsen
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "Medarbejder oprettet",
            Entity = "Employee",
            EntityId = employee.Id.ToString(),
            UserId = CurrentUserId,
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _db.SaveChangesAsync();

        // Hent den oprettede medarbejder med navigation properties
        await _db.Entry(employee).Reference(e => e.Location).LoadAsync();

        var createdRoles = await _userManager.GetRolesAsync(employee);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, MapToResponseDto(employee, createdRoles.ToList()));
    }

    // PUT /api/employees/{id}
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<EmployeeResponseDto>> Update(int id, EmployeeUpdateDto dto)
    {
        var employee = await _userManager.Users
            .Include(e => e.Location)
            .Include(e => e.Authorization)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
            return NotFound();

        var locationExists = await _db.Locations.AnyAsync(l => l.LocationID == dto.LocationID);
        if (!locationExists)
            return BadRequest($"Location med ID {dto.LocationID} findes ikke.");

        var authorizationExists = await _db.Authorizations.AnyAsync(a => a.AuthorizationID == dto.AuthorizationID);
        if (!authorizationExists)
            return BadRequest($"Authorization med ID {dto.AuthorizationID} findes ikke.");

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.UserName = dto.Email;
        employee.PhoneNumber = dto.PhoneNumber;
        employee.LocationID = dto.LocationID;
        employee.AuthorizationID = dto.AuthorizationID;

        // Opdater pincode hvis der er angivet en ny
        if (!string.IsNullOrEmpty(dto.NewPincode))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(employee);
            var resetResult = await _userManager.ResetPasswordAsync(employee, resetToken, dto.NewPincode);
            if (!resetResult.Succeeded)
                return BadRequest(resetResult.Errors);
        }

        await _userManager.UpdateAsync(employee);

        // Log opdateringen
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "Medarbejder opdateret",
            Entity = "Employee",
            EntityId = employee.Id.ToString(),
            UserId = CurrentUserId,
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _db.SaveChangesAsync();

        return Ok(MapToResponseDto(employee));
    }

    // DELETE /api/employees/{id} — GDPR: permanent sletning af medarbejder og al tilknyttet data
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _userManager.FindByIdAsync(id.ToString());

        if (employee is null)
            return NotFound();

        var name = $"{employee.FirstName} {employee.LastName}";

        await _userManager.DeleteAsync(employee);

        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType  = "GDPR",
            Action   = $"GDPR sletning: Medarbejder '{name}' (ID {id}) og al tilknyttet data er permanent slettet.",
            Entity   = "Employee",
            EntityId = id.ToString(),
            UserId   = null,
            UserName = "unknown"
        });
        await _db.SaveChangesAsync();

        return NoContent();
    }


    // GET /api/employees/job-roles — Hent tilgængelige ansvarsområder
    [HttpGet("job-roles")]
    [Authorize(Policy = "RequireCareStaff")]
    public async Task<ActionResult<IEnumerable<object>>> GetJobRoles()
    {
        var roles = await _db.ResponsibilityRoles
            .Select(r => new { r.RoleID, r.Name, r.ResponsibilityArea })
            .ToListAsync();
        return Ok(roles);
    }
    // PUT /api/employees/{id}/job-roles — Tildel ansvarsomraader
    [HttpPut("{id}/job-roles")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> UpdateJobRoles(int id, [FromBody] List<string> jobRoleNames)
    {
        var employee = await _userManager.Users
            .Include(e => e.Roles)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
            return NotFound();

        // Hent alle tilgaengelige Role-entiteter (ansvarsomraader)
        var allJobRoles = await _db.ResponsibilityRoles.ToListAsync();
        var validNames = allJobRoles.Select(r => r.Name).ToHashSet();

        // Valider
        foreach (var name in jobRoleNames)
        {
            if (!validNames.Contains(name))
                return BadRequest($"Ugyldigt ansvarsomraade: {name}. Gyldige: {string.Join(", ", validNames)}");
        }

        // Fjern eksisterende, tilfoej nye
        employee.Roles.Clear();
        foreach (var name in jobRoleNames)
        {
            var role = allJobRoles.First(r => r.Name == name);
            employee.Roles.Add(role);
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Ansvarsomraader opdateret." });
    }

    // --------------------------------------------------------
    //  Hjælpemetode: mapper Employee entity → ResponseDto
    // --------------------------------------------------------
    private static EmployeeResponseDto MapToResponseDto(Employee e, List<string>? identityRoles = null) => new()
    {
        Id = e.Id,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email!,
        PhoneNumber = e.PhoneNumber!,
        HasPincode = e.HasPincode,
        LocationID = e.LocationID,
        LocationName = e.Location?.Name ?? string.Empty,
        Roles = identityRoles ?? new List<string>(),
        JobRoles = e.Roles.Select(r => r.Name).ToList()
    };
}
