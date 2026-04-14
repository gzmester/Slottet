// ============================================================
//  EmployeesController
//
//  Håndterer alle HTTP requests vedrørende medarbejdere.
//
//  Endpoints:
//    GET    /api/employees        → Henter alle medarbejdere
//    GET    /api/employees/{id}   → Henter én medarbejder
//    POST   /api/employees        → Opretter en ny medarbejder
//    PUT    /api/employees/{id}   → Opdaterer en medarbejder
//    DELETE /api/employees/{id}   → Sletter en medarbejder
// ============================================================

using Application.DTOs.Employee;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EmployeesController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET /api/employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetAll()
    {
        var employees = await _db.Employees
            .Include(e => e.Location)
            .Include(e => e.Roles)
            .ToListAsync();

        var result = employees.Select(MapToResponseDto);

        return Ok(result);
    }

    // GET /api/employees/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeResponseDto>> GetById(int id)
    {
        var employee = await _db.Employees
            .Include(e => e.Location)
            .Include(e => e.Roles)
            .FirstOrDefaultAsync(e => e.EmployeeID == id);

        if (employee is null)
            return NotFound();

        return Ok(MapToResponseDto(employee));
    }

    // POST /api/employees
    [HttpPost]
    public async Task<ActionResult<EmployeeResponseDto>> Create(EmployeeCreateDto dto)
    {
        if (!Enum.TryParse<ShiftType>(dto.ShiftType, ignoreCase: true, out var shiftType))
            return BadRequest($"Ugyldig ShiftType: '{dto.ShiftType}'. Gyldige værdier: {string.Join(", ", Enum.GetNames<ShiftType>())}");

        var locationExists = await _db.Locations.AnyAsync(l => l.LocationID == dto.LocationID);
        if (!locationExists)
            return BadRequest($"Location med ID {dto.LocationID} findes ikke.");

        var authorizationExists = await _db.Authorizations.AnyAsync(a => a.AuthorizationID == dto.AuthorizationID);
        if (!authorizationExists)
            return BadRequest($"Authorization med ID {dto.AuthorizationID} findes ikke.");

        var employee = new Employee
        {
            FirstName       = dto.FirstName,
            LastName        = dto.LastName,
            Email           = dto.Email,
            PhoneNumber     = dto.PhoneNumber,
            ShiftType       = shiftType,
            PinCode         = dto.PinCode,
            LocationID      = dto.LocationID,
            AuthorizationID = dto.AuthorizationID
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        // Hent den oprettede medarbejder med navigation properties
        await _db.Entry(employee).Reference(e => e.Location).LoadAsync();
        await _db.Entry(employee).Collection(e => e.Roles).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = employee.EmployeeID }, MapToResponseDto(employee));
    }

    // PUT /api/employees/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<EmployeeResponseDto>> Update(int id, EmployeeUpdateDto dto)
    {
        var employee = await _db.Employees
            .Include(e => e.Location)
            .Include(e => e.Roles)
            .FirstOrDefaultAsync(e => e.EmployeeID == id);

        if (employee is null)
            return NotFound();

        if (!Enum.TryParse<ShiftType>(dto.ShiftType, ignoreCase: true, out var shiftType))
            return BadRequest($"Ugyldig ShiftType: '{dto.ShiftType}'. Gyldige værdier: {string.Join(", ", Enum.GetNames<ShiftType>())}");

        var locationExists = await _db.Locations.AnyAsync(l => l.LocationID == dto.LocationID);
        if (!locationExists)
            return BadRequest($"Location med ID {dto.LocationID} findes ikke.");

        var authorizationExists = await _db.Authorizations.AnyAsync(a => a.AuthorizationID == dto.AuthorizationID);
        if (!authorizationExists)
            return BadRequest($"Authorization med ID {dto.AuthorizationID} findes ikke.");

        employee.FirstName       = dto.FirstName;
        employee.LastName        = dto.LastName;
        employee.Email           = dto.Email;
        employee.PhoneNumber     = dto.PhoneNumber;
        employee.ShiftType       = shiftType;
        employee.PinCode         = dto.PinCode;
        employee.LocationID      = dto.LocationID;
        employee.AuthorizationID = dto.AuthorizationID;

        await _db.SaveChangesAsync();

        return Ok(MapToResponseDto(employee));
    }

    // DELETE /api/employees/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _db.Employees.FindAsync(id);

        if (employee is null)
            return NotFound();

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // --------------------------------------------------------
    //  Hjælpemetode: mapper Employee entity → ResponseDto
    // --------------------------------------------------------
    private static EmployeeResponseDto MapToResponseDto(Employee e) => new()
    {
        EmployeeID      = e.EmployeeID,
        FirstName       = e.FirstName,
        LastName        = e.LastName,
        Email           = e.Email,
        PhoneNumber     = e.PhoneNumber,
        ShiftType       = e.ShiftType.ToString(),
        PinCode         = e.PinCode,
        LocationID      = e.LocationID,
        LocationName    = e.Location?.Name ?? string.Empty,
        AuthorizationID = e.AuthorizationID,
        Roles           = e.Roles.Select(r => r.Name).ToList()
    };
}
