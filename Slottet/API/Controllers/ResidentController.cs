using Application.DTOs.Resident;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ResidentController : ControllerBase
    {
        private readonly IResidentRepository _repo;
        private readonly IAuditLogRepository _auditLog;

        public ResidentController(IResidentRepository repo, IAuditLogRepository auditLog)
        {
            _repo     = repo;
            _auditLog = auditLog;
        }

        private int? CurrentUserId => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
        private string CurrentUserName => User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResidentResponseDto>>> GetAll()
        {
            var residents = await _repo.GetAllAsync();
            return Ok(residents.Select(MapToResponseDto));
        }

        [HttpGet("public")]
        public async Task<ActionResult<IEnumerable<ResidentPublicDto>>> GetPublic([FromQuery] int? locationId)
        {
            var residents = await _repo.GetPublicAsync(locationId);

            var result = residents.Select(r => new ResidentPublicDto
            {
                ResidentID = r.ResidentID,
                LocationID = r.LocationID,
                RiskLevel  = r.RiskLevel.ToString(),
                Mood       = r.Mood.ToString(),
                Medicins   = r.Medicins
                    .OrderBy(m => m.Time.TimeOfDay)
                    .Select(m => new MedicinPublicDto
                    {
                        Time    = m.Time.ToString("HH:mm"),
                        IsTaken = m.IsTaken
                    })
                    .ToList()
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResidentResponseDto>> GetById(int id)
        {
            var resident = await _repo.GetByIdAsync(id);

            if (resident == null)
                return NotFound($"Resident with ID {id} not found");

            return Ok(MapToResponseDto(resident));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireCareStaff")]
        public async Task<ActionResult<ResidentResponseDto>> Update(int id, UpdateResidentRequestDto updateDto)
        {
            
            var resident = await _repo.GetByIdWithStatusesAsync(id);

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
                    ResidentID  = resident.ResidentID,
                    Description = updateDto.Status,
                    Time        = DateTime.Now
                };
                _repo.AddStatus(newStatus);
            }

            _repo.Update(resident);
            await _repo.SaveChangesAsync();

            if (oldRiskLevel != resident.RiskLevel)
            {
                await _auditLog.AddAsync(new AuditLog
                {
                    LogType  = "Activity",
                    Action   = "Risikoniveau opdateret",
                    Entity   = "Resident",
                    EntityId = id.ToString(),
                    UserId   = CurrentUserId,
                    UserName = CurrentUserName
                });
            }

            if (oldMood != resident.Mood)
            {
                await _auditLog.AddAsync(new AuditLog
                {
                    LogType  = "Activity",
                    Action   = "Humør opdateret",
                    Entity   = "Resident",
                    EntityId = id.ToString(),
                    UserId   = CurrentUserId,
                    UserName = CurrentUserName
                });
            }

            await _auditLog.SaveChangesAsync();

            // Reload to get updated relationships
            resident = await _repo.GetByIdAsync(id);

            return Ok(MapToResponseDto(resident!));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var resident = await _repo.GetByIdAsync(id);

            if (resident == null)
                return NotFound($"Resident with ID {id} not found");

            var name = $"{resident.FirstName} {resident.LastName}";

            _repo.Remove(resident);
            await _repo.SaveChangesAsync();

            await _auditLog.AddAsync(new AuditLog
            {
                LogType  = "GDPR",
                Action   = $"GDPR sletning: Borger '{name}' (ID {id}) og al tilknyttet data er permanent slettet.",
                Entity   = "Resident",
                EntityId = id.ToString(),
                UserId   = CurrentUserId,
                UserName = CurrentUserName
            });
            await _auditLog.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ResidentResponseDto>> Create(ResidentCreateDto createDto)
        {
            var resident = new Resident
            {
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Room = createDto.Room,
                ShoppingDay = createDto.ShoppingDay,
                Payment = createDto.Payment,
                LocationID = createDto.LocationID,
                RiskLevel = createDto.RiskLevel,
                Mood = createDto.Mood
            };

            _repo.Add(resident);
            await _repo.SaveChangesAsync();

            await _auditLog.AddAsync(new AuditLog
            {
                LogType  = "Activity",
                Action   = "Borger oprettet",
                Entity   = "Resident",
                EntityId = resident.ResidentID.ToString(),
                UserId   = CurrentUserId,
                UserName = CurrentUserName
            });
            await _auditLog.SaveChangesAsync();

            var created = await _repo.GetByIdAsync(resident.ResidentID);

            return CreatedAtAction(nameof(GetById), new { id = resident.ResidentID }, MapToResponseDto(created!));

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
            //Map medicin til medicindto for at undgå beboerid i response
            Medicins = resident.Medicins.Select(m => new MedicinDto
            {
                MedicinID = m.MedicinID,
                Type      = m.Type,
                Time      = m.Time,
                IsTaken   = m.IsTaken
            }).ToList(),
            //Map pn medicin til pnmedicinresponse dto for at undgå beboerid i response
            PNMedicins = resident.PNMedicins.Select(pn => new PNMedicinResponseDto
            {
                PNMedicinID = pn.PNMedicinID,
                Type        = pn.Type,
                Time        = pn.Time
            }).ToList(),
            //Map status til statusdto for at undgå beboerid i response, og filtrere så kun dagens statusser returneres
            Statuses = resident.Statuses
                .Where(s => s.Time.Date == DateTime.Today)
                .OrderByDescending(s => s.Time)
                .Select(s => new StatusDto { Description = s.Description, Time = s.Time })
                .ToList()
            
        };
        
        
        

    }
}
