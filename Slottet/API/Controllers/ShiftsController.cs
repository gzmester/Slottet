using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ShiftsController(ApplicationDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // POST /api/shifts — Opret eller opdater dagens vagt for den indloggede medarbejder
    [HttpPost]
    [Authorize(Policy = "RequireCareStaff")]
    public async Task<IActionResult> CreateShift([FromBody] CreateShiftDto dto)
    {
        var employeeId = dto.EmployeeId > 0 ? dto.EmployeeId : CurrentUserId;

        // Kun Admin kan oprette vagter for andre
        if (employeeId != CurrentUserId && !User.IsInRole("Admin"))
            return Forbid();

        // Valider ShiftType
        if (!Enum.IsDefined(typeof(ShiftType), dto.ShiftType))
            return BadRequest($"Ugyldig vagttype. Gyldige: {string.Join(", ", Enum.GetNames<ShiftType>())}");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Tjek om der allerede er en vagt i dag for denne medarbejder
        var existingShift = await _db.Shifts
            .FirstOrDefaultAsync(s => s.EmployeeID == employeeId && s.Date == today);

        if (existingShift != null)
        {
            // Opdater eksisterende vagt
            existingShift.ShiftType = dto.ShiftType;
            await _db.SaveChangesAsync();
            return Ok(new { shiftId = existingShift.ShiftID, shiftType = existingShift.ShiftType.ToString(), date = existingShift.Date.ToString("yyyy-MM-dd") });
        }

        // Opret ny vagt
        var shift = new Shift
        {
            ShiftType = dto.ShiftType,
            Date = today,
            EmployeeID = employeeId
        };

        _db.Shifts.Add(shift);
        await _db.SaveChangesAsync();

        return Ok(new { shiftId = shift.ShiftID, shiftType = shift.ShiftType.ToString(), date = shift.Date.ToString("yyyy-MM-dd") });
    }

    // GET /api/shifts/today — Hent dagens vagt for den indloggede medarbejder
    [HttpGet("today")]
    [Authorize(Policy = "RequireCareStaff")]
    public async Task<IActionResult> GetTodayShift()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var shift = await _db.Shifts
            .FirstOrDefaultAsync(s => s.EmployeeID == CurrentUserId && s.Date == today);

        if (shift == null)
            return Ok(new { shiftType = (string?)null });

        return Ok(new { shiftId = shift.ShiftID, shiftType = shift.ShiftType.ToString(), date = shift.Date.ToString("yyyy-MM-dd") });
    }
}

public class CreateShiftDto
{
    public ShiftType ShiftType { get; set; }
    public int EmployeeId { get; set; }
}
