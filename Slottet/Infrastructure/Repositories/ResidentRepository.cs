using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ResidentRepository : IResidentRepository
{
    private readonly ApplicationDbContext _db;

    public ResidentRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Resident>> GetAllAsync() =>
        await _db.Residents
            .Include(r => r.Location)
            .Include(r => r.Medicins)
            .Include(r => r.PNMedicins)
            .Include(r => r.Statuses)
            .ToListAsync();

    public async Task<IEnumerable<Resident>> GetPublicAsync(int? locationId)
    {
        var query = _db.Residents.Include(r => r.Medicins).AsQueryable();
        if (locationId.HasValue)
            query = query.Where(r => r.LocationID == locationId.Value);
        return await query.ToListAsync();
    }

    public async Task<Resident?> GetByIdAsync(int id) =>
        await _db.Residents
            .Include(r => r.Location)
            .Include(r => r.Medicins)
            .Include(r => r.PNMedicins)
            .Include(r => r.Statuses)
            .FirstOrDefaultAsync(r => r.ResidentID == id);

    public async Task<Resident?> GetByIdWithStatusesAsync(int id) =>
        await _db.Residents
            .Include(r => r.Statuses)
            .FirstOrDefaultAsync(r => r.ResidentID == id);

    public void Add(Resident resident) => _db.Residents.Add(resident);
    public void Update(Resident resident) => _db.Residents.Update(resident);
    public void Remove(Resident resident) => _db.Residents.Remove(resident);
    public void AddStatus(Status status) => _db.Statuses.Add(status);

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
}
