using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IDepartmentTasksRepository
{
    Task<IEnumerable<DepartmentTask>> GetAllAsync();
    Task<DepartmentTask?> GetByIdAsync(int id);
    void Add(DepartmentTask task);
    void Remove(DepartmentTask task);
    Task<int> SaveChangesAsync();
}
