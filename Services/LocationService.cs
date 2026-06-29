using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class LocationService
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public LocationService(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<List<Location>> GetAllAsync(string? search = null)
    {
        var query = _db.Locations.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l => l.Name.Contains(search) ||
                                     (l.Address != null && l.Address.Contains(search)));
        return await query.OrderBy(l => l.Name).ToListAsync();
    }

    public async Task<Location?> GetByIdAsync(int id) => await _db.Locations.FindAsync(id);

    public async Task<int> CountEventsAsync(int locationId) =>
        await _db.Events.CountAsync(e => e.LocationId == locationId);

    public async Task<Location> CreateAsync(Location location, string actorUserId)
    {
        _db.Locations.Add(location);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Create", "Location", location.LocationId, $"Created {location.Name}");
        return location;
    }

    public async Task UpdateAsync(Location location, string actorUserId)
    {
        _db.Locations.Update(location);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Location", location.LocationId, $"Updated {location.Name}");
    }

    public async Task<bool> DeleteAsync(int id, string actorUserId)
    {
        var location = await _db.Locations.FindAsync(id);
        if (location is null) return false;

        var inUse = await _db.Events.AnyAsync(e => e.LocationId == id);
        if (inUse) return false; // caller should show an error

        location.Deleted = true;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "Location", id, $"Deleted {location.Name}");
        return true;
    }
}
