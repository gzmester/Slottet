using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Application.DTOs.Resident;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PNMedicinController : ControllerBase
{
    private readonly IPNMedicinRepository _repo;
    private readonly IAuditLogRepository _auditLog;

    public PNMedicinController(IPNMedicinRepository repo, IAuditLogRepository auditLog)
    {
        _repo     = repo;
        _auditLog = auditLog;
    }

    // PUT /api/pnmedicin/{id}
    //endpoint til at opdatere en pn medicin, bruges når en medarbejder ændre i en eksisterede pn medicin for en beboer
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePNMedicin(int id, [FromBody] UpdatePNMedicinRequest request)
    {
        //Find pn medicinen i databasen
        var pnmedicin = await _repo.GetByIdAsync(id);

        if (pnmedicin is null)
            return NotFound();

        pnmedicin.Time = request.Time;
        pnmedicin.Type = request.Type;
        await _repo.SaveChangesAsync();

        await _auditLog.AddAsync(new AuditLog
        {
            LogType  = "Activity",
            Action   = "PN-Medicin tid opdateret",
            Entity   = "PN-Medicin",
            EntityId = id.ToString(),
            UserId   = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _auditLog.SaveChangesAsync();

        return NoContent();
    }

    //POST /api/medicin
    //post endpoint til at oprette en PN medicin for en beboer
    [HttpPost]
    public async Task<ActionResult<PNMedicinResponseDto>> Create(PNMedicinCreateDto dto)
    {
        //Opret en ny PN medicin baseret på DTO'en
        var pnmedicin = new PNMedicin
        {
            Type       = dto.Type,
            ResidentID = dto.ResidentId,
            Time       = dto.Time,
        };

        //Gem PN i databasen
        _repo.Add(pnmedicin);
        await _repo.SaveChangesAsync();

        await _auditLog.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "PN-Medicin Tilføjet",
            Entity = "PN-Medicin",
            EntityId = pnmedicin.PNMedicinID.ToString(),
            UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _auditLog.SaveChangesAsync();

        //Returnere den oprettede PN medicin som dto
        return Ok(MapToResponseDto(pnmedicin));
    }

    //Hjælpe metode til at mappe PN medicin til response dto
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