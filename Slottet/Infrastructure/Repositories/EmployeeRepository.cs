using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _db;

    public EmployeeRepository(ApplicationDbContext db) => _db = db;

    public async Task LoadLocationAsync(Employee employee) =>
        await _db.Entry(employee).Reference(e => e.Location).LoadAsync();

    public async Task<IEnumerable<Role>> GetJobRolesAsync() =>
        await _db.ResponsibilityRoles.ToListAsync();

    public async Task<IEnumerable<Role>> GetJobRolesByNamesAsync(IEnumerable<string> names) =>
        await _db.ResponsibilityRoles
            .Where(r => names.Contains(r.Name))
            .ToListAsync();

    public async Task<Employee?> GetWithRolesAsync(int id) =>
        await _db.Employees
            .Include(e => e.Roles)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task ClearAndSetJobRolesAsync(Employee employee, IEnumerable<Role> newRoles)
    {
        employee.Roles.Clear();
        foreach (var role in newRoles)
            employee.Roles.Add(role);
        await _db.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
}
