using Microsoft.AspNetCore.Http;
using Application.DTOs.Resident;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ResidentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ResidentController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResidentResponseDto>>> GetAll()
        {
            var residents = await _db.Residents
                .Include(r => r.Location)
                .Include(r => r.Medicins)
                .Include(r => r.PNMedicins)
                .Include(r => r.Statuses)
                .ToListAsync();

            var result = residents.Select(MapToResponseDto);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResidentResponseDto>> GetById(int id)
        {
            var resident = await _db.Residents
                .Include(r => r.Location)
                .Include(r => r.Medicins)
                .Include(r => r.PNMedicins)
                .Include(r => r.Statuses)
                .FirstOrDefaultAsync(r => r.ResidentID == id);

            if (resident == null)
                return NotFound($"Resident with ID {id} not found");

            return Ok(MapToResponseDto(resident));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResidentResponseDto>> Update(int id, UpdateResidentRequestDto updateDto)
        {
            
            var resident = await _db.Residents
                .Include(r => r.Statuses)
                .FirstOrDefaultAsync(r => r.ResidentID == id);

            if (resident == null)
                return NotFound($"Resident with ID {id} not found");

            var oldRiskLevel = resident.RiskLevel;
            var oldMood = resident.Mood;

            // Update fields
            resident.FirstName = updateDto.FirstName;
            resident.LastName = updateDto.LastName;
            resident.Room = updateDto.Room;
            resident.ShoppingDay = updateDto.ShoppingDay;
            resident.Payment = updateDto.Payment;

            // Parse RiskLevel enum
            if (Enum.TryParse<RiskLevel>(updateDto.RiskLevel, out var riskLevel))
            {
                resident.RiskLevel = riskLevel;
            }

            // Parse Mood enum
            if (Enum.TryParse<Mood>(updateDto.Mood, out var mood))
            {
                resident.Mood = mood;
            }

            // Handle Status update - create a new status entry if provided
            if (!string.IsNullOrWhiteSpace(updateDto.Status))
            {
                var newStatus = new Status
                {
                    ResidentID = resident.ResidentID,
                    Description = updateDto.Status,
                    Time = DateTime.Now
                };
                _db.Statuses.Add(newStatus);
            }

            _db.Residents.Update(resident);
            await _db.SaveChangesAsync();

            if( oldRiskLevel != resident.RiskLevel)
            {
                await _db.AuditLogs.AddAsync(new AuditLog
                {
                    LogType  = "Activity",
                    Action   = "Risikoniveau opdateret",
                    Entity   = "Resident",
                    EntityId = id.ToString(),
                    UserId   = null,
                    UserName = "unknown"
                });
            }

            if(oldMood != resident.Mood)
            {
                await _db.AuditLogs.AddAsync(new AuditLog
                {
                    LogType  = "Activity",
                    Action   = "Humør opdateret",
                    Entity   = "Resident",
                    EntityId = id.ToString(),
                    UserId   = null,
                    UserName = "unknown"
                });
            }

            await _db.SaveChangesAsync();

            // Reload to get updated relationships
            resident = await _db.Residents
                .Include(r => r.Location)
                .Include(r => r.Medicins)
                .Include(r => r.PNMedicins)
                .Include(r => r.Statuses)
                .FirstOrDefaultAsync(r => r.ResidentID == id);

            return Ok(MapToResponseDto(resident!));
        }

        private static ResidentResponseDto MapToResponseDto(Resident resident) => new ()
        {
            
            ResidentID = resident.ResidentID,
            FirstName = resident.FirstName,
            LastName = resident.LastName,
            Room = resident.Room,
            RiskLevel = resident.RiskLevel.ToString(),
            Mood = resident.Mood.ToString(),
            ShoppingDay = resident.ShoppingDay,
            Payment = resident.Payment,
            LocationID = resident.LocationID,
            Location = resident.Location.Name,
            Medicins = resident.Medicins.Select(m => new MedicinDto
            {
                MedicinID = m.MedicinID,
                Type      = m.Type,
                Time      = m.Time,
                IsTaken   = m.IsTaken
            }).ToList(),
            Statuses = resident.Statuses
                .Where(s => s.Time.Date == DateTime.Today)
                .OrderByDescending(s => s.Time)
                .Select(s => new StatusDto { Description = s.Description, Time = s.Time })
                .ToList()
            
        };
        
        
        

    }
}
