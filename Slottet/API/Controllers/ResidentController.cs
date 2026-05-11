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
            ShoppingDay = resident.ShoppingDay,
            Payment = resident.Payment,
            LocationID = resident.LocationID,
            Location = resident.Location.Name,
            //Medicins = resident.Medicins.Select(m => m.Name).ToList(),
            //PNMedicins = resident.PNMedicins.Select(m => m.Name).ToList(),
            Statuses = resident.Statuses.Select(s => s.Description).ToList()
            
        };
        
        
        

    }
}
