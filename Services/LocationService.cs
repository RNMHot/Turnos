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

    public async Task<List<LocationPosition>> GetPositionsAsync(int locationId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.LocationPositions
            .Where(p => p.LocationId == locationId)
            .OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<LocationPosition> AddPositionAsync(int locationId, string name, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var maxOrder = await db.LocationPositions
            .Where(p => p.LocationId == locationId)
            .MaxAsync(p => (int?)p.DisplayOrder) ?? 0;
        var pos = new LocationPosition { LocationId = locationId, Name = name.Trim(), DisplayOrder = maxOrder + 1 };
        db.LocationPositions.Add(pos);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Create", "LocationPosition", pos.LocationPositionId, $"Added position '{pos.Name}' to location {locationId}");
        return pos;
    }

    public async Task MovePositionAsync(int locationId, int positionId, bool moveUp, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var list = await db.LocationPositions
            .Where(p => p.LocationId == locationId && !p.Deleted)
            .OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name)
            .ToListAsync();

        var idx = list.FindIndex(p => p.LocationPositionId == positionId);
        var swapIdx = moveUp ? idx - 1 : idx + 1;
        if (idx < 0 || swapIdx < 0 || swapIdx >= list.Count) return;

        (list[idx].DisplayOrder, list[swapIdx].DisplayOrder) = (list[swapIdx].DisplayOrder, list[idx].DisplayOrder);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "LocationPosition", positionId, $"Reordered position '{list[idx].Name}'");
    }

    public async Task DeletePositionAsync(int positionId, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var pos = await db.LocationPositions.FindAsync(positionId);
        if (pos is null) return;
        await db.Assignments
            .Where(a => a.LocationPositionId == positionId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.LocationPositionId, (int?)null));
        pos.Deleted = true;
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "LocationPosition", positionId, $"Deleted position '{pos.Name}'");
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
