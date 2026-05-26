using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AuthorizationRepository : IAuthorizationRepository
{
    private readonly ApplicationDbContext _db;

    public AuthorizationRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Authorization>> GetAllAsync() =>
        await _db.Authorizations.ToListAsync();

    public async Task<bool> ExistsAsync(int id) =>
        await _db.Authorizations.AnyAsync(a => a.AuthorizationID == id);
}
