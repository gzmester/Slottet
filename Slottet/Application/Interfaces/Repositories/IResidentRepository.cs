using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IResidentRepository
{
    Task<IEnumerable<Resident>> GetAllAsync();
    Task<IEnumerable<Resident>> GetPublicAsync(int? locationId);
    Task<Resident?> GetByIdAsync(int id);
    Task<Resident?> GetByIdWithStatusesAsync(int id);
    void Add(Resident resident);
    void Update(Resident resident);
    void Remove(Resident resident);
    void AddStatus(Status status);
    Task<int> SaveChangesAsync();
}
