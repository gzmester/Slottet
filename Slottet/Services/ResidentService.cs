using Microsoft.EntityFrameworkCore;
using Slottet.Data;
using Slottet.Models;

namespace Slottet.Services;

public class ResidentService : IResidentService
{
    private readonly ApplicationDbContext _context;

    public ResidentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Resident>> GetAllResidentsAsync()
    {
        return await _context.Residents
            .Include(r => r.Medications)
            .ToListAsync();
    }

    public async Task<Resident?> GetResidentByIdAsync(int id)
    {
        return await _context.Residents
            .Include(r => r.Medications)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Resident> CreateResidentAsync(Resident resident)
    {
        resident.CreatedAt = DateTime.UtcNow;
        _context.Residents.Add(resident);
        await _context.SaveChangesAsync();
        return resident;
    }

    public async Task<Resident?> UpdateResidentAsync(int id, Resident resident)
    {
        var existingResident = await _context.Residents.FindAsync(id);
        if (existingResident == null)
            return null;

        existingResident.Name = resident.Name;
        existingResident.CPR = resident.CPR;
        existingResident.DateOfBirth = resident.DateOfBirth;
        existingResident.RoomNumber = resident.RoomNumber;
        existingResident.MedicalNotes = resident.MedicalNotes;
        existingResident.IsActive = resident.IsActive;
        existingResident.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existingResident;
    }

    public async Task<bool> DeleteResidentAsync(int id)
    {
        var resident = await _context.Residents.FindAsync(id);
        if (resident == null)
            return false;

        _context.Residents.Remove(resident);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Resident>> GetActiveResidentsAsync()
    {
        return await _context.Residents
            .Where(r => r.IsActive)
            .Include(r => r.Medications)
            .ToListAsync();
    }
}
