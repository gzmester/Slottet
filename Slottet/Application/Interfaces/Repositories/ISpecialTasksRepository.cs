using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ISpecialTasksRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByIdWithEmployeesAsync(int id);
    Task<IEnumerable<Employee>> GetEmployeesByIdsAsync(IEnumerable<int> ids);
    void Add(Role role);
    void Remove(Role role);
    Task<int> SaveChangesAsync();
}
