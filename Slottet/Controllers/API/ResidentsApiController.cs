using Microsoft.AspNetCore.Mvc;
using Slottet.Models;
using Slottet.Services;

namespace Slottet.Controllers.API;

[ApiController]
[Route("api/residents")]
public class ResidentsApiController : ControllerBase
{
    private readonly IResidentService _residentService;
    private readonly ILogger<ResidentsApiController> _logger;

    public ResidentsApiController(IResidentService residentService, ILogger<ResidentsApiController> logger)
    {
        _residentService = residentService;
        _logger = logger;
    }

    /// <summary>
    /// Henter alle beboere
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Resident>>> GetAllResidents()
    {
        try
        {
            var residents = await _residentService.GetAllResidentsAsync();
            return Ok(residents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all residents");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Henter aktive beboere
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Resident>>> GetActiveResidents()
    {
        try
        {
            var residents = await _residentService.GetActiveResidentsAsync();
            return Ok(residents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active residents");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Henter en specifik beboer
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Resident>> GetResident(int id)
    {
        try
        {
            var resident = await _residentService.GetResidentByIdAsync(id);
            if (resident == null)
                return NotFound($"Resident with ID {id} not found");

            return Ok(resident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resident {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Opretter en ny beboer
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Resident>> CreateResident([FromBody] Resident resident)
    {
        try
        {
            var createdResident = await _residentService.CreateResidentAsync(resident);
            return CreatedAtAction(nameof(GetResident), new { id = createdResident.Id }, createdResident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resident");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Opdaterer en beboer
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Resident>> UpdateResident(int id, [FromBody] Resident resident)
    {
        try
        {
            var updatedResident = await _residentService.UpdateResidentAsync(id, resident);
            if (updatedResident == null)
                return NotFound($"Resident with ID {id} not found");

            return Ok(updatedResident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resident {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Sletter en beboer
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteResident(int id)
    {
        try
        {
            var result = await _residentService.DeleteResidentAsync(id);
            if (!result)
                return NotFound($"Resident with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resident {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
