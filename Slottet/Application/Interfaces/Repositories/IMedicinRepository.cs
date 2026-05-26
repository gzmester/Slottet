using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IMedicinRepository
{
    Task<Medicin?> GetByIdAsync(int id);
    void Add(Medicin medicin);
    Task<int> SaveChangesAsync();
}
