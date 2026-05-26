using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task AddAsync(AuditLog log);
    Task<int> SaveChangesAsync();
}
