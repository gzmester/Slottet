using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MedicinRepository : IMedicinRepository
{
    private readonly ApplicationDbContext _db;

    public MedicinRepository(ApplicationDbContext db) => _db = db;

    public async Task<Medicin?> GetByIdAsync(int id) =>
        await _db.Medicins.FindAsync(id);

    public void Add(Medicin medicin) => _db.Medicins.Add(medicin);

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
}
