using Microsoft.AspNetCore.Http;
using Application.DTOs.Resident;
using Domain.Entities;
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
           // Medicins = resident.Medicins.Select(m => m.Name).ToList(),
            //PNMedicins = resident.PNMedicins.Select(m => m.Name).ToList(),
            Statuses = resident.Statuses.Select(s => s.Description).ToList()
            
        };
        
        
        

    }
}
