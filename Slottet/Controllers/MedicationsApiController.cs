using Microsoft.AspNetCore.Mvc;
using Slottet.Models;
using Slottet.Services;

namespace Slottet.Controllers.API;

[ApiController]
[Route("api/medications")]
public class MedicationsApiController : ControllerBase
{
    private readonly IMedicationService _medicationService;
    private readonly ILogger<MedicationsApiController> _logger;

    public MedicationsApiController(IMedicationService medicationService, ILogger<MedicationsApiController> logger)
    {
        _medicationService = medicationService;
        _logger = logger;
    }

    /// <summary>
    /// Henter medicin for en beboer
    /// </summary>
    [HttpGet("resident/{residentId}")]
    public async Task<ActionResult<IEnumerable<Medication>>> GetMedicationsByResident(int residentId)
    {
        try
        {
            var medications = await _medicationService.GetMedicationsByResidentIdAsync(residentId);
            return Ok(medications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medications for resident {ResidentId}", residentId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Henter specifik medicin
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Medication>> GetMedication(int id)
    {
        try
        {
            var medication = await _medicationService.GetMedicationByIdAsync(id);
            if (medication == null)
                return NotFound($"Medication with ID {id} not found");

            return Ok(medication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medication {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Opretter ny medicin
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Medication>> CreateMedication([FromBody] Medication medication)
    {
        try
        {
            var createdMedication = await _medicationService.CreateMedicationAsync(medication);
            return CreatedAtAction(nameof(GetMedication), new { id = createdMedication.Id }, createdMedication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating medication");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Opdaterer medicin
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Medication>> UpdateMedication(int id, [FromBody] Medication medication)
    {
        try
        {
            var updatedMedication = await _medicationService.UpdateMedicationAsync(id, medication);
            if (updatedMedication == null)
                return NotFound($"Medication with ID {id} not found");

            return Ok(updatedMedication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating medication {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Sletter medicin
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMedication(int id)
    {
        try
        {
            var result = await _medicationService.DeleteMedicationAsync(id);
            if (!result)
                return NotFound($"Medication with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting medication {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Logger at medicin er givet
    /// </summary>
    [HttpPost("log")]
    public async Task<ActionResult<MedicationLog>> LogAdministration([FromBody] MedicationLog log)
    {
        try
        {
            var createdLog = await _medicationService.LogMedicationAdministrationAsync(log);
            return Ok(createdLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging medication administration");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Henter medicin log
    /// </summary>
    [HttpGet("{id}/logs")]
    public async Task<ActionResult<IEnumerable<MedicationLog>>> GetMedicationLogs(int id)
    {
        try
        {
            var logs = await _medicationService.GetMedicationLogsAsync(id);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting medication logs for {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}
