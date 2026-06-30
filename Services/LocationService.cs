using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class LocationService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly AuditService _audit;

    public LocationService(IDbContextFactory<AppDbContext> dbFactory, AuditService audit)
    {
        _dbFactory = dbFactory;
        _audit = audit;
    }

    public async Task<List<Location>> GetAllAsync(string? search = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Locations.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l => l.Name.Contains(search) ||
                                     (l.Address != null && l.Address.Contains(search)));
        return await query.OrderBy(l => l.Name).ToListAsync();
    }

    public async Task<Location?> GetByIdAsync(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Locations.FindAsync(id);
    }

    public async Task<int> CountEventsAsync(int locationId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Events.CountAsync(e => e.LocationId == locationId);
    }

    public async Task<Location> CreateAsync(Location location, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Locations.Add(location);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Create", "Location", location.LocationId, $"Created {location.Name}");
        return location;
    }

    public async Task UpdateAsync(Location location, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Locations.Update(location);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Location", location.LocationId, $"Updated {location.Name}");
    }

    public async Task<bool> DeleteAsync(int id, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var location = await db.Locations.FindAsync(id);
        if (location is null) return false;

        var inUse = await db.Events.AnyAsync(e => e.LocationId == id);
        if (inUse) return false;

        location.Deleted = true;
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "Location", id, $"Deleted {location.Name}");
        return true;
    }
}
