using Application.DTOs.AuditLog;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AuditLogController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET /api/auditlog
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogResponseDto>>> GetAll()
    {
        //henter alle logs fra databasen sorteret efter faldende dato fra den nyeste
        // Så nyeste logs vises først
        var logs = await _db.AuditLogs
            .OrderByDescending(log => log.TimeStamp)
            //mapper entity til DTO
            .Select(log => new AuditLogResponseDto
            {
                AuditId = log.AuditId,
                LogType = log.LogType,
                Action = log.Action,
                Entity = log.Entity,
                EntityId = log.EntityId,
                UserId = log.UserId,
                UserName = log.UserName,
                TimeStamp = log.TimeStamp
            })
            .ToListAsync();

        return Ok(logs);
    }
}