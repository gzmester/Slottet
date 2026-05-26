using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ShiftRepository : IShiftRepository
{
    private readonly ApplicationDbContext _db;

    public ShiftRepository(ApplicationDbContext db) => _db = db;

    public async Task<Shift?> GetTodayForEmployeeAsync(int employeeId, DateOnly date) =>
        await _db.Shifts.FirstOrDefaultAsync(s => s.EmployeeID == employeeId && s.Date == date);

    public void Add(Shift shift) => _db.Shifts.Add(shift);

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
}
