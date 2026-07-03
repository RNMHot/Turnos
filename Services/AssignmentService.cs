using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class AssignmentService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly AuditService _audit;
    private readonly AvailabilityService _availability;
    private readonly AppSettingsState _settings;

    public AssignmentService(IDbContextFactory<AppDbContext> dbFactory, AuditService audit, AvailabilityService availability, AppSettingsState settings)
    {
        _dbFactory = dbFactory;
        _audit = audit;
        _availability = availability;
        _settings = settings;
    }

    public async Task<List<Assignment>> GetAllAsync(int? eventId = null, int? personId = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Assignments
            .Include(a => a.Person).ThenInclude(p => p.PersonRoles).ThenInclude(pr => pr.Role)
            .Include(a => a.Event).ThenInclude(e => e.Company)
            .Include(a => a.LocationPosition)
            .Include(a => a.Role)
            .AsQueryable();

        if (eventId.HasValue) query = query.Where(a => a.EventId == eventId);
        if (personId.HasValue) query = query.Where(a => a.PersonId == personId);

        return await query.OrderBy(a => a.StartDateTime).ToListAsync();
    }

    public async Task<Assignment?> GetByIdAsync(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Assignments
            .Include(a => a.Person).ThenInclude(p => p.Availabilities)
            .Include(a => a.Event)
            .Include(a => a.LocationPosition)
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.AssignmentId == id);
    }

    private DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : _settings.ToUtc(value);

    public async Task<(bool Success, string Error)> CreateAsync(Assignment assignment, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        assignment.StartDateTime = ToUtc(assignment.StartDateTime);
        assignment.EndDateTime = ToUtc(assignment.EndDateTime);

        if (await HasOverlapAsync(db, assignment.PersonId, assignment.StartDateTime, assignment.EndDateTime))
            return (false, "Esta persona ya está asignada a este evento.");

        var person = await db.Persons
            .Include(p => p.Availabilities)
            .Include(p => p.PersonRoles).ThenInclude(pr => pr.Role)
            .FirstOrDefaultAsync(p => p.PersonId == assignment.PersonId);
        if (person is not null && !_availability.IsPersonAvailable(person, assignment.StartDateTime, assignment.EndDateTime))
            return (false, "Esta persona no está disponible en este momento.");

        if (assignment.RoleId is null && person is not null)
            assignment.RoleId = GetHighestRankRoleId(person);

        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Create", "Assignment", assignment.AssignmentId,
            $"Assigned person {assignment.PersonId} to event {assignment.EventId}");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(Assignment assignment, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        assignment.StartDateTime = ToUtc(assignment.StartDateTime);
        assignment.EndDateTime = ToUtc(assignment.EndDateTime);

        if (await HasOverlapAsync(db, assignment.PersonId, assignment.StartDateTime, assignment.EndDateTime, assignment.AssignmentId))
            return (false, "Esta persona ya está asignada a este evento.");

        db.Assignments.Update(assignment);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Assignment", assignment.AssignmentId, "Updated assignment");

        return (true, string.Empty);
    }

    public async Task DeleteAsync(int id, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var a = await db.Assignments.FindAsync(id);
        if (a is null) return;
        a.Deleted = true;
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "Assignment", id, "Deleted assignment");
    }

    public async Task UpdateStatusAsync(int id, AssignmentStatus status, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var a = await db.Assignments.FindAsync(id);
        if (a is null) return;
        a.Status = status;
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Assignment", id, $"Status changed to {status}");
    }

    public async Task UpdatePositionAsync(int id, int? locationPositionId, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var a = await db.Assignments.FindAsync(id);
        if (a is null) return;
        a.LocationPositionId = locationPositionId;
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Assignment", id, "Position changed");
    }

    public async Task UpdateRoleAsync(int id, int? roleId, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var a = await db.Assignments.FindAsync(id);
        if (a is null) return;
        a.RoleId = roleId;
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Assignment", id, "Role changed for this event");
    }

    public static int? GetHighestRankRoleId(Person person) =>
        person.PersonRoles
            .Where(pr => !pr.Deleted)
            .OrderBy(pr => pr.Role.Rank)
            .Select(pr => (int?)pr.RoleId)
            .FirstOrDefault();

    private static async Task<bool> HasOverlapAsync(AppDbContext db, int personId, DateTime start, DateTime end, int? excludeId = null)
    {
        var query = db.Assignments.Where(a =>
            a.PersonId == personId &&
            !a.Deleted &&
            a.StartDateTime < end && a.EndDateTime > start);

        if (excludeId.HasValue)
            query = query.Where(a => a.AssignmentId != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> IsValidAssignment(int personId, DateTime start, DateTime end, int? excludeId = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        if (await HasOverlapAsync(db, personId, start, end, excludeId)) return false;
        var person = await db.Persons.Include(p => p.Availabilities).FirstOrDefaultAsync(p => p.PersonId == personId);
        if (person is null) return false;
        return _availability.IsPersonAvailable(person, start, end);
    }
}
