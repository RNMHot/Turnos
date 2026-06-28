using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class EventService
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public EventService(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<List<Event>> GetAllAsync(string? search = null, int? companyId = null,
        DateTime? from = null, DateTime? to = null, bool? active = null)
    {
        var query = _db.Events
            .Include(e => e.Company)
            .Include(e => e.Location)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => e.EventName.Contains(search) ||
                                     (e.Location != null && e.Location.Name.Contains(search)));
        if (companyId.HasValue)
            query = query.Where(e => e.CompanyId == companyId);
        if (from.HasValue)
            query = query.Where(e => e.StartDateTime >= from.Value);
        if (to.HasValue)
            query = query.Where(e => e.StartDateTime <= to.Value);
        if (active.HasValue)
            query = query.Where(e => e.Active == active.Value);

        return await query.OrderBy(e => e.StartDateTime).ToListAsync();
    }

    public async Task<List<Event>> GetForWeekAsync(DateTime weekStart)
    {
        var localWeekStart = weekStart.Date;
        var localWeekEnd = localWeekStart.AddDays(7);

        var events = await _db.Events
            .Include(e => e.Company)
            .Include(e => e.Location)
            .Include(e => e.Assignments).ThenInclude(a => a.Person)
            .Where(e => e.Active)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();

        return events
            .Where(e =>
            {
                var start = NormalizeToLocal(e.StartDateTime);
                var end = NormalizeToLocal(e.EndDateTime);
                return start < localWeekEnd && end > localWeekStart;
            })
            .ToList();
    }

    public async Task<List<Event>> GetActiveWithAssignmentsAsync()
    {
        return await _db.Events
            .Include(e => e.Company)
            .Include(e => e.Location)
            .Include(e => e.Assignments).ThenInclude(a => a.Person)
            .Where(e => e.Active)
            .OrderBy(e => e.StartDateTime)
            .ToListAsync();
    }

    private static DateTime NormalizeToLocal(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value.ToLocalTime(),
            DateTimeKind.Local => value,
            _ => DateTime.SpecifyKind(value, DateTimeKind.Local)
        };
    }

    public async Task<Event?> GetByIdAsync(int id) =>
        await _db.Events
            .Include(e => e.Company)
            .Include(e => e.Location)
            .Include(e => e.Assignments).ThenInclude(a => a.Person).ThenInclude(p => p.PersonRoles).ThenInclude(pr => pr.Role)
            .FirstOrDefaultAsync(e => e.EventId == id);

    public async Task<Event> CreateAsync(Event ev, string actorUserId)
    {
        ev.StartDateTime = ToUtc(ev.StartDateTime);
        ev.EndDateTime = ToUtc(ev.EndDateTime);
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Create", "Event", ev.EventId, $"Created {ev.EventName}");
        return ev;
    }

    public async Task UpdateAsync(Event ev, string actorUserId)
    {
        ev.StartDateTime = ToUtc(ev.StartDateTime);
        ev.EndDateTime = ToUtc(ev.EndDateTime);
        _db.Events.Update(ev);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Event", ev.EventId, $"Updated {ev.EventName}");
    }

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
        };
    }

    public async Task DeactivateAsync(int id, string actorUserId)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null) return;
        ev.Active = false;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "Event", id, $"Deactivated {ev.EventName}");
    }
}
