using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PhoneNumberRepository : IPhoneNumberRepository
{
    private readonly ApplicationDbContext _db;

    public PhoneNumberRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<PhoneNumber>> GetAllAsync() =>
        await _db.PhoneNumbers.ToListAsync();

    public async Task<PhoneNumber?> GetByIdAsync(int id) =>
        await _db.PhoneNumbers.FindAsync(id);

    public void Add(PhoneNumber phoneNumber) => _db.PhoneNumbers.Add(phoneNumber);
    public void Remove(PhoneNumber phoneNumber) => _db.PhoneNumbers.Remove(phoneNumber);

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
}
