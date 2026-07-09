using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

// Tracks login/logout history plus real-time connection state (via TurnosCircuitHandler)
// so the "Sesiones de usuario" admin page can show who is currently connected.
public class UserSessionService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public UserSessionService(IDbContextFactory<AppDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<int> StartSessionAsync(string userId, string email, string? personName, string? ipAddress)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        // A stale session (browser closed without hitting /signout) can be left open from
        // a previous login; close it out before starting the new one.
        var stale = await db.UserSessions
            .Where(s => s.UserId == userId && s.LogoutAt == null)
            .ToListAsync();
        foreach (var s in stale)
            s.LogoutAt = s.LastActivityAt;

        var session = new UserSession
        {
            UserId = userId,
            Email = email,
            PersonName = personName,
            IpAddress = ipAddress,
            LoginAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
        };
        db.UserSessions.Add(session);
        await db.SaveChangesAsync();
        return session.UserSessionId;
    }

    public async Task EndSessionAsync(string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var open = await db.UserSessions
            .Where(s => s.UserId == userId && s.LogoutAt == null)
            .ToListAsync();
        var now = DateTime.UtcNow;
        foreach (var s in open)
        {
            s.LogoutAt = now;
            s.LastActivityAt = now;
            s.ConnectedCircuits = 0;
        }
        await db.SaveChangesAsync();
    }

    public async Task CircuitOpenedAsync(string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var session = await db.UserSessions
            .Where(s => s.UserId == userId && s.LogoutAt == null)
            .OrderByDescending(s => s.LoginAt)
            .FirstOrDefaultAsync();
        if (session is null) return;

        session.ConnectedCircuits++;
        session.LastActivityAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task CircuitClosedAsync(string userId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var session = await db.UserSessions
            .Where(s => s.UserId == userId && s.LogoutAt == null)
            .OrderByDescending(s => s.LoginAt)
            .FirstOrDefaultAsync();
        if (session is null) return;

        session.ConnectedCircuits = Math.Max(0, session.ConnectedCircuits - 1);
        session.LastActivityAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<List<UserSession>> GetSessionsAsync(DateTime? from = null, DateTime? to = null, bool onlyConnected = false)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.UserSessions.AsNoTracking().AsQueryable();

        if (from.HasValue)
            query = query.Where(s => s.LoginAt >= from.Value);
        if (to.HasValue)
            query = query.Where(s => s.LoginAt <= to.Value);
        if (onlyConnected)
            query = query.Where(s => s.LogoutAt == null && s.ConnectedCircuits > 0);

        return await query
            .OrderByDescending(s => s.LoginAt)
            .Take(500)
            .ToListAsync();
    }
}
