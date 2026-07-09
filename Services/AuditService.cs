using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class AuditService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AuditService(IDbContextFactory<AppDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task LogAsync(string userId, string action, string entityType, int entityId, string? details = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Timestamp = DateTime.UtcNow,
            Details = details
        });
        await db.SaveChangesAsync();
    }

    public async Task<List<AuditLog>> GetLogsAsync(string? userId = null, string? entityType = null,
        DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 50)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(a => a.UserId == userId);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);
        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userIds = logs.Select(a => a.UserId).Distinct().ToList();
        var userNames = await db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Email ?? u.Id);

        foreach (var log in logs)
            log.UserName = userNames.GetValueOrDefault(log.UserId, log.UserId);

        return logs;
    }
}
