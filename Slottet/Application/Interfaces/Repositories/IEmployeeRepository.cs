using Domain.Entities;

namespace Application.Interfaces.Repositories;

/// <summary>
/// Håndterer de DbContext-operationer i EmployeesController der ikke dækkes af UserManager.
/// UserManager&lt;Employee&gt; forbliver en separat afhængighed, da det er en Identity-abstraktion.
/// </summary>
public interface IEmployeeRepository
{
    Task LoadLocationAsync(Employee employee);
    Task<IEnumerable<Role>> GetJobRolesAsync();
    Task<IEnumerable<Role>> GetJobRolesByNamesAsync(IEnumerable<string> names);
    Task<Employee?> GetWithRolesAsync(int id);
    Task ClearAndSetJobRolesAsync(Employee employee, IEnumerable<Role> newRoles);
    Task<int> SaveChangesAsync();
}
