using Microsoft.EntityFrameworkCore;
using Slottet.Data;
using Slottet.Models;

namespace Slottet.Services;

public class MedicationService : IMedicationService
{
    private readonly ApplicationDbContext _context;

    public MedicationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Medication>> GetMedicationsByResidentIdAsync(int residentId)
    {
        return await _context.Medications
            .Where(m => m.ResidentId == residentId && m.IsActive)
            .Include(m => m.Logs)
            .ToListAsync();
    }

    public async Task<Medication?> GetMedicationByIdAsync(int id)
    {
        return await _context.Medications
            .Include(m => m.Resident)
            .Include(m => m.Logs)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Medication> CreateMedicationAsync(Medication medication)
    {
        medication.CreatedAt = DateTime.UtcNow;
        _context.Medications.Add(medication);
        await _context.SaveChangesAsync();
        return medication;
    }

    public async Task<Medication?> UpdateMedicationAsync(int id, Medication medication)
    {
        var existingMedication = await _context.Medications.FindAsync(id);
        if (existingMedication == null)
            return null;

        existingMedication.Name = medication.Name;
        existingMedication.Dosage = medication.Dosage;
        existingMedication.Frequency = medication.Frequency;
        existingMedication.Instructions = medication.Instructions;
        existingMedication.StartDate = medication.StartDate;
        existingMedication.EndDate = medication.EndDate;
        existingMedication.IsActive = medication.IsActive;
        existingMedication.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingMedication;
    }

    public async Task<bool> DeleteMedicationAsync(int id)
    {
        var medication = await _context.Medications.FindAsync(id);
        if (medication == null)
            return false;

        _context.Medications.Remove(medication);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MedicationLog> LogMedicationAdministrationAsync(MedicationLog log)
    {
        log.AdministeredAt = DateTime.UtcNow;
        _context.MedicationLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<IEnumerable<MedicationLog>> GetMedicationLogsAsync(int medicationId)
    {
        return await _context.MedicationLogs
            .Where(ml => ml.MedicationId == medicationId)
            .Include(ml => ml.AdministeredBy)
            .OrderByDescending(ml => ml.AdministeredAt)
            .ToListAsync();
    }
}
