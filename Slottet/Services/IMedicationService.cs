using Slottet.Models;

namespace Slottet.Services;

public interface IMedicationService
{
    Task<IEnumerable<Medication>> GetMedicationsByResidentIdAsync(int residentId);
    Task<Medication?> GetMedicationByIdAsync(int id);
    Task<Medication> CreateMedicationAsync(Medication medication);
    Task<Medication?> UpdateMedicationAsync(int id, Medication medication);
    Task<bool> DeleteMedicationAsync(int id);
    Task<MedicationLog> LogMedicationAdministrationAsync(MedicationLog log);
    Task<IEnumerable<MedicationLog>> GetMedicationLogsAsync(int medicationId);
}
