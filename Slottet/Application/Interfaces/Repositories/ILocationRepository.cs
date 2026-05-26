using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ILocationRepository
{
    Task<IEnumerable<Location>> GetAllAsync();
    Task<Location?> GetByIdAsync(int id);
    Task<bool> ExistsAsync(int id);
    void Add(Location location);
    void Remove(Location location);
    Task<int> SaveChangesAsync();
}
