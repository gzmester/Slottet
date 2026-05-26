using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DepartmentTasksRepository : IDepartmentTasksRepository
{
    private readonly ApplicationDbContext _db;

    public DepartmentTasksRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<DepartmentTask>> GetAllAsync() =>
        await _db.DepartmentTasks.ToListAsync();

    public async Task<DepartmentTask?> GetByIdAsync(int id) =>
        await _db.DepartmentTasks.FindAsync(id);

    public void Add(DepartmentTask task) => _db.DepartmentTasks.Add(task);
    public void Remove(DepartmentTask task) => _db.DepartmentTasks.Remove(task);

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
}
