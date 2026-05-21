using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Application.DTOs.Resident;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PNMedicinController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public PNMedicinController(ApplicationDbContext db)
    {
        _db = db;
    }

    // PUT /api/pnmedicin/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePNMedicin(int id, [FromBody] UpdatePNMedicinRequest request)
    {
        var pnmedicin = await _db.PNMedicins.FindAsync(id);

        if (pnmedicin is null)
            return NotFound();

        pnmedicin.Time = request.Time;
        pnmedicin.Type = request.Type;
        await _db.SaveChangesAsync();

        //Log pnmedicin time updated
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType  = "Activity",
            Action   = "PN-Medicin tid opdateret",
            Entity   = "PN-Medicin",
            EntityId = id.ToString(),
            UserId   = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _db.SaveChangesAsync();

        return NoContent();
    }

    //POST /api/medicin
    [HttpPost]
    public async Task<ActionResult<PNMedicinResponseDto>> Create(PNMedicinCreateDto dto)
    {
        var pnmedicin = new PNMedicin
        {
            Type       = dto.Type,
            ResidentID = dto.ResidentId,
            Time       = dto.Time,
        };

        _db.PNMedicins.Add(pnmedicin);
        await _db.SaveChangesAsync();

        //Log oprettelsen i AuditLog
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "PN-Medicin Tilføjet",
            Entity = "PN-Medicin",
            EntityId = pnmedicin.PNMedicinID.ToString(),
            UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _db.SaveChangesAsync();

        return Ok(MapToResponseDto(pnmedicin));
    }

    private static PNMedicinResponseDto MapToResponseDto(PNMedicin m) => new()
    {
        PNMedicinID = m.PNMedicinID,
        Type = m.Type,
        Time = m.Time,
    };
}

public class UpdatePNMedicinRequest
{
    public DateTime Time { get; set; }
    public string Type { get; set; } = string.Empty;
}