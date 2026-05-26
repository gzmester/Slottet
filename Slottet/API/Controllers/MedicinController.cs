using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Application.DTOs.Resident;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicinController : ControllerBase
{
    private readonly IMedicinRepository _repo;
    private readonly IAuditLogRepository _auditLog;

    public MedicinController(IMedicinRepository repo, IAuditLogRepository auditLog)
    {
        _repo     = repo;
        _auditLog = auditLog;
    }

    //POST /api/medicin
    //post endpoint til at oprette en medicin for en beboer
    [HttpPost]
    public async Task<ActionResult<MedicinDto>> Create(MedicinCreateDto dto)
    {
        //Opret en ny medicin baseret på DTO'en
        var medicin = new Medicin
        {
            Type       = dto.Type,
            ResidentID = dto.ResidentId,
            Time       = DateTime.Now,
            IsTaken    = false
        };

        //Gem medicinen i databasen
        _repo.Add(medicin);
        await _repo.SaveChangesAsync();

        //Log oprettelsen i AuditLog
        await _auditLog.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "Medicin oprettet",
            Entity = "Medicin",
            EntityId = medicin.MedicinID.ToString(),
            UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _auditLog.SaveChangesAsync();

        //retturnere den oprettede medicin som DTO
        return Ok(MapToResponseDto(medicin));
    }

    // PATCH /api/medicin/{id}/taken?isTaken=true
    [HttpPatch("{id}/taken")]
    public async Task<IActionResult> SetTaken(int id, [FromQuery] bool isTaken)
    {
        var medicin = await _repo.GetByIdAsync(id);

        if (medicin is null)
            return NotFound();

        medicin.IsTaken = isTaken;
        await _repo.SaveChangesAsync();

        //Log medicin taget
        await _auditLog.AddAsync(new AuditLog
        {
            LogType  = "Activity",
            Action   = isTaken ? "Medicin taget" : "Medicin fortrudt",
            Entity   = "Medicin",
            EntityId = id.ToString(),
            UserId   = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _auditLog.SaveChangesAsync();

        return NoContent();

        
    }

    // PUT /api/medicin/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedicin(int id, [FromBody] UpdateMedicinRequest request)
    {
        var medicin = await _repo.GetByIdAsync(id);

        if (medicin is null)
            return NotFound();

        medicin.Time = request.Time;
        await _repo.SaveChangesAsync();

        //Log medicin time updated
        await _auditLog.AddAsync(new AuditLog
        {
            LogType  = "Activity",
            Action   = "Medicin tid opdateret",
            Entity   = "Medicin",
            EntityId = id.ToString(),
            UserId   = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _auditLog.SaveChangesAsync();

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
