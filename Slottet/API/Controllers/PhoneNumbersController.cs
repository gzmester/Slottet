using Application.DTOs.PhoneNumber;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]

public class PhoneNumberController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public PhoneNumberController(ApplicationDbContext db)
    {
        _db = db;
    }

    // Get /api/phonenumbers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PhoneNumberResponseDto>>> GetAll()
    {
        var phoneNumbers = await _db.PhoneNumbers.ToListAsync();

        var result = phoneNumbers.Select(MapToResponseDto);

        return Ok(result);

    }

    // GET /api/phonenumbers/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<PhoneNumberResponseDto>> GetById(int id)
    {
        var phoneNumber = await _db.PhoneNumbers.FirstOrDefaultAsync(p => p.Id == id);

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

        _db.PhoneNumbers.Add(phoneNumber);
        await _db.SaveChangesAsync();

        //Log oprettelsen i AuditLog
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "Telefonnummer oprettet",
            Entity = "PhoneNumber",
            EntityId = phoneNumber.Id.ToString(),
            UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = phoneNumber.Id }, MapToResponseDto(phoneNumber));
    }

    // PUT /api/phonenumbers/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<PhoneNumberResponseDto>> Update(int id, PhoneNumberUpdateDto dto)
    {
        var phoneNumber = await _db.PhoneNumbers.FirstOrDefaultAsync(p => p.Id == id);

        if (phoneNumber is null) return NotFound();

        phoneNumber.Number = dto.Number;

        await _db.SaveChangesAsync();

        //Log opdateringen i AuditLog
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "Telefonnummer opdateret",
            Entity = "PhoneNumber",
            EntityId = phoneNumber.Id.ToString(),
            UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _db.SaveChangesAsync();

        return Ok(MapToResponseDto(phoneNumber));
    }

    // PATCH /api/phonenumbers/{id}/assignment
    [HttpPatch("{id}/assignment")]
    public async Task<ActionResult<PhoneNumberResponseDto>> UpdateAssignment(int id, [FromBody] int? assignedTo)
    {
        var phoneNumber = await _db.PhoneNumbers.FirstOrDefaultAsync(p => p.Id == id);

        if(phoneNumber is null) return NotFound();

        phoneNumber.AssignedTo = assignedTo;
        await _db.SaveChangesAsync();

        //Log opdateringen i AuditLog
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "Tilkoblet medarbejder til telefonnummer",
            Entity = "PhoneNumber",
            EntityId = phoneNumber.Id.ToString(),
            UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _db.SaveChangesAsync();

        return Ok(MapToResponseDto(phoneNumber));
    }

    // DELETE /api/phonenumbers/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var phoneNumber = await _db.PhoneNumbers.FirstOrDefaultAsync(p => p.Id == id);

        if(phoneNumber is null) return NotFound();

        _db.PhoneNumbers.Remove(phoneNumber);
        await _db.SaveChangesAsync();

        //Log sletningen i AuditLog
        await _db.AuditLogs.AddAsync(new AuditLog
        {
            LogType = "Activity",
            Action = "Telefonnummer slettet",
            Entity = "PhoneNumber",
            EntityId = phoneNumber.Id.ToString(),
            UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
            UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown"
        });
        await _db.SaveChangesAsync();

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