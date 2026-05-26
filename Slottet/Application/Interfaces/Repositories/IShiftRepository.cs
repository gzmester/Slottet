using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IShiftRepository
{
    Task<Shift?> GetTodayForEmployeeAsync(int employeeId, DateOnly date);
    void Add(Shift shift);
    Task<int> SaveChangesAsync();
}
