using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PNMedicinRepository : IPNMedicinRepository
{
    private readonly ApplicationDbContext _db;

    public PNMedicinRepository(ApplicationDbContext db) => _db = db;

    public async Task<PNMedicin?> GetByIdAsync(int id) =>
        await _db.PNMedicins.FindAsync(id);

    public void Add(PNMedicin pnMedicin) => _db.PNMedicins.Add(pnMedicin);

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
}
