using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IPNMedicinRepository
{
    Task<PNMedicin?> GetByIdAsync(int id);
    void Add(PNMedicin pnMedicin);
    Task<int> SaveChangesAsync();
}
