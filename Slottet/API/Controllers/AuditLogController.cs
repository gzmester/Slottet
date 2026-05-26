using Application.DTOs.AuditLog;
using Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogRepository _repo;

    public AuditLogController(IAuditLogRepository repo)
    {
        _repo = repo;
    }

    // GET /api/auditlog
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogResponseDto>>> GetAll()
    {
        var logs = await _repo.GetAllAsync();

        return Ok(logs.Select(log => new AuditLogResponseDto
        {
            AuditId   = log.AuditId,
            LogType   = log.LogType,
            Action    = log.Action,
            Entity    = log.Entity,
            EntityId  = log.EntityId,
            UserId    = log.UserId,
            UserName  = log.UserName,
            TimeStamp = log.TimeStamp
        }));
    }
}