using Application.DTOs.PhoneNumber;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]

public class PhoneNumberController : ControllerBase
{
    private readonly IPhoneNumberRepository _repo;
    private readonly IAuditLogRepository _auditLog;

    public PhoneNumberController(IPhoneNumberRepository repo, IAuditLogRepository auditLog)
    {
        _repo     = repo;
        _auditLog = auditLog;
    }

    // Get /api/phonenumbers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PhoneNumberResponseDto>>> GetAll()
    {
        var phoneNumbers = await _repo.GetAllAsync();

        var result = phoneNumbers.Select(MapToResponseDto);

        return Ok(result);

    }

    // GET /api/phonenumbers/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<PhoneNumberResponseDto>> GetById(int id)
    {
        var phoneNumber = await _repo.GetByIdAsync(id);

        if(phoneNumber is null) return NotFound();

        return Ok(MapToResponseDto(phoneNumber));
    }

    // Post /api/phonenumbers
    [HttpPost]
    public async Task<ActionResult<PhoneNumberResponseDto>> Create(PhoneNumberCreateDto dto)
    {
        var phoneNumber = new PhoneNumber
        {
            Number = dto.Number
        };

        _repo.Add(phoneNumber);
        await _repo.SaveChangesAsync();

        await _auditLog.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "Telefonnummer oprettet",
            Entity = "PhoneNumber",
            EntityId = phoneNumber.Id.ToString(),
            UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _auditLog.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = phoneNumber.Id }, MapToResponseDto(phoneNumber));
    }

    // PUT /api/phonenumbers/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<PhoneNumberResponseDto>> Update(int id, PhoneNumberUpdateDto dto)
    {
        var phoneNumber = await _repo.GetByIdAsync(id);

        if (phoneNumber is null) return NotFound();

        phoneNumber.Number = dto.Number;

        await _repo.SaveChangesAsync();

        await _auditLog.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "Telefonnummer opdateret",
            Entity = "PhoneNumber",
            EntityId = phoneNumber.Id.ToString(),
            UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _auditLog.SaveChangesAsync();

        return Ok(MapToResponseDto(phoneNumber));
    }

    // PATCH /api/phonenumbers/{id}/assignment
    [HttpPatch("{id}/assignment")]
    public async Task<ActionResult<PhoneNumberResponseDto>> UpdateAssignment(int id, [FromBody] int? assignedTo)
    {
        var phoneNumber = await _repo.GetByIdAsync(id);

        if(phoneNumber is null) return NotFound();

        phoneNumber.AssignedTo = assignedTo;
        await _repo.SaveChangesAsync();

        await _auditLog.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "Tilkoblet medarbejder til telefonnummer",
            Entity = "PhoneNumber",
            EntityId = phoneNumber.Id.ToString(),
            UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _auditLog.SaveChangesAsync();

        return Ok(MapToResponseDto(phoneNumber));
    }

    // DELETE /api/phonenumbers/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var phoneNumber = await _repo.GetByIdAsync(id);

        if(phoneNumber is null) return NotFound();

        _repo.Remove(phoneNumber);
        await _repo.SaveChangesAsync();

        await _auditLog.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "Telefonnummer slettet",
            Entity = "PhoneNumber",
            EntityId = phoneNumber.Id.ToString(),
            UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _auditLog.SaveChangesAsync();

        return NoContent();
    }

    //Hjælpemetode til at mappe phonenumber entity til responseDto
    private static PhoneNumberResponseDto MapToResponseDto(PhoneNumber p) => new()
    {
        Id = p.Id,
        Number = p.Number,
        AssignedTo = p.AssignedTo
    };


}