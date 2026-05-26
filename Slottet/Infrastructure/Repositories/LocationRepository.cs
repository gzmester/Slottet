using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly ApplicationDbContext _db;

    public LocationRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Location>> GetAllAsync() =>
        await _db.Locations.ToListAsync();

    public async Task<Location?> GetByIdAsync(int id) =>
        await _db.Locations.FindAsync(id);

    public async Task<bool> ExistsAsync(int id) =>
        await _db.Locations.AnyAsync(l => l.LocationID == id);

    public void Add(Location location) => _db.Locations.Add(location);
    public void Remove(Location location) => _db.Locations.Remove(location);

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
}
