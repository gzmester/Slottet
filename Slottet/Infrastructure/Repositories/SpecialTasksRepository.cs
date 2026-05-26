using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SpecialTasksRepository : ISpecialTasksRepository
{
    private readonly ApplicationDbContext _db;

    public SpecialTasksRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Role>> GetAllAsync() =>
        await _db.ResponsibilityRoles
            .Include(r => r.Employees)
            .ToListAsync();

    public async Task<Role?> GetByIdWithEmployeesAsync(int id) =>
        await _db.ResponsibilityRoles
            .Include(r => r.Employees)
            .FirstOrDefaultAsync(r => r.RoleID == id);

    public async Task<IEnumerable<Employee>> GetEmployeesByIdsAsync(IEnumerable<int> ids) =>
        await _db.Employees
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();

    public void Add(Role role) => _db.ResponsibilityRoles.Add(role);
    public void Remove(Role role) => _db.ResponsibilityRoles.Remove(role);

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
}
