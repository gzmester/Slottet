using Slottet.Models;

namespace Slottet.Services;

public interface IResidentService
{
    Task<IEnumerable<Resident>> GetAllResidentsAsync();
    Task<Resident?> GetResidentByIdAsync(int id);
    Task<Resident> CreateResidentAsync(Resident resident);
    Task<Resident?> UpdateResidentAsync(int id, Resident resident);
    Task<bool> DeleteResidentAsync(int id);
    Task<IEnumerable<Resident>> GetActiveResidentsAsync();
}
