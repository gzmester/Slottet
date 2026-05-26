using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _db;

    public AuditLogRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<AuditLog>> GetAllAsync() =>
        await _db.AuditLogs
            .OrderByDescending(a => a.TimeStamp)
            .ToListAsync();

    public async Task AddAsync(AuditLog log) =>
        await _db.AuditLogs.AddAsync(log);

    public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
}
