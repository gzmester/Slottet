using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftRepository _repo;

    public ShiftsController(IShiftRepository repo)
    {
        _repo = repo;
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
        var existingShift = await _repo.GetTodayForEmployeeAsync(employeeId, today);

        if (existingShift != null)
        {
            // Opdater eksisterende vagt
            existingShift.ShiftType = dto.ShiftType;
            await _repo.SaveChangesAsync();
            return Ok(new { shiftId = existingShift.ShiftID, shiftType = existingShift.ShiftType.ToString(), date = existingShift.Date.ToString("yyyy-MM-dd") });
        }

        // Opret ny vagt
        var shift = new Shift
        {
            ShiftType = dto.ShiftType,
            Date = today,
            EmployeeID = employeeId
        };

        _repo.Add(shift);
        await _repo.SaveChangesAsync();

        return Ok(new { shiftId = shift.ShiftID, shiftType = shift.ShiftType.ToString(), date = shift.Date.ToString("yyyy-MM-dd") });
    }

    // GET /api/shifts/today — Hent dagens vagt for den indloggede medarbejder
    [HttpGet("today")]
    [Authorize(Policy = "RequireCareStaff")]
    public async Task<IActionResult> GetTodayShift()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var shift = await _repo.GetTodayForEmployeeAsync(CurrentUserId, today);

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
