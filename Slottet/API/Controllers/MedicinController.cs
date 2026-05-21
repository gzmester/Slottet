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
public class MedicinController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public MedicinController(ApplicationDbContext db)
    {
        _db = db;
    }

    //POST /api/medicin
    [HttpPost]
    public async Task<ActionResult<MedicinDto>> Create(MedicinCreateDto dto)
    {
        var medicin = new Medicin
        {
            Type       = dto.Type,
            ResidentID = dto.ResidentId,
            Time       = DateTime.Now,
            IsTaken    = false
        };

        _db.Medicins.Add(medicin);
        await _db.SaveChangesAsync();

        //Log oprettelsen i AuditLog
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "Medicin oprettet",
            Entity = "Medicin",
            EntityId = medicin.MedicinID.ToString(),
            UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _db.SaveChangesAsync();

        return Ok(MapToResponseDto(medicin));
    }

    // PATCH /api/medicin/{id}/taken?isTaken=true
    [HttpPatch("{id}/taken")]
    public async Task<IActionResult> SetTaken(int id, [FromQuery] bool isTaken)
    {
        var medicin = await _db.Medicins.FindAsync(id);

        if (medicin is null)
            return NotFound();

        medicin.IsTaken = isTaken;
        await _db.SaveChangesAsync();

        //Log medicin taget
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType  = "Activity",
            Action   = isTaken ? "Medicin taget" : "Medicin fortrudt",
            Entity   = "Medicin",
            EntityId = id.ToString(),
            UserId   = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _db.SaveChangesAsync();

        return NoContent();

        
    }

    // PUT /api/medicin/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedicin(int id, [FromBody] UpdateMedicinRequest request)
    {
        var medicin = await _db.Medicins.FindAsync(id);

        if (medicin is null)
            return NotFound();

        medicin.Time = request.Time;
        await _db.SaveChangesAsync();

        //Log medicin time updated
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType  = "Activity",
            Action   = "Medicin tid opdateret",
            Entity   = "Medicin",
            EntityId = id.ToString(),
            UserId   = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static MedicinDto MapToResponseDto(Medicin m) => new()
    {
        MedicinID = m.MedicinID,
        Type = m.Type,
        Time = m.Time,
        IsTaken = m.IsTaken
    };
}

public class UpdateMedicinRequest
{
    public DateTime Time { get; set; }
}
