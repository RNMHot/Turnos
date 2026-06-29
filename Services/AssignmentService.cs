using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class AssignmentService
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;
    private readonly AvailabilityService _availability;

    public AssignmentService(AppDbContext db, AuditService audit, AvailabilityService availability)
    {
        _db = db;
        _audit = audit;
        _availability = availability;
    }

    public async Task<List<Assignment>> GetAllAsync(int? eventId = null, int? personId = null)
    {
        var query = _db.Assignments
            .Include(a => a.Person).ThenInclude(p => p.PersonRoles).ThenInclude(pr => pr.Role)
            .Include(a => a.Event).ThenInclude(e => e.Company)
            .AsQueryable();

        if (eventId.HasValue) query = query.Where(a => a.EventId == eventId);
        if (personId.HasValue) query = query.Where(a => a.PersonId == personId);

        return await query.OrderBy(a => a.StartDateTime).ToListAsync();
    }

    public async Task<Assignment?> GetByIdAsync(int id) =>
        await _db.Assignments
            .Include(a => a.Person).ThenInclude(p => p.Availabilities)
            .Include(a => a.Event)
            .FirstOrDefaultAsync(a => a.AssignmentId == id);

    public async Task<(bool Success, string Error)> CreateAsync(Assignment assignment, string actorUserId)
    {
        if (await HasOverlapAsync(assignment.PersonId, assignment.StartDateTime, assignment.EndDateTime))
            return (false, "This person already has an overlapping assignment.");

        var person = await _db.Persons.Include(p => p.Availabilities).FirstOrDefaultAsync(p => p.PersonId == assignment.PersonId);
        if (person is not null && !_availability.IsPersonAvailable(person, assignment.StartDateTime, assignment.EndDateTime))
            return (false, "This person is not available at this time.");

        assignment.StartDateTime = DateTime.SpecifyKind(assignment.StartDateTime, DateTimeKind.Utc);
        assignment.EndDateTime = DateTime.SpecifyKind(assignment.EndDateTime, DateTimeKind.Utc);

        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Create", "Assignment", assignment.AssignmentId,
            $"Assigned person {assignment.PersonId} to event {assignment.EventId}");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(Assignment assignment, string actorUserId)
    {
        if (await HasOverlapAsync(assignment.PersonId, assignment.StartDateTime, assignment.EndDateTime, assignment.AssignmentId))
            return (false, "This person already has an overlapping assignment.");

        assignment.StartDateTime = DateTime.SpecifyKind(assignment.StartDateTime, DateTimeKind.Utc);
        assignment.EndDateTime = DateTime.SpecifyKind(assignment.EndDateTime, DateTimeKind.Utc);

        _db.Assignments.Update(assignment);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Assignment", assignment.AssignmentId, "Updated assignment");

        return (true, string.Empty);
    }

    public async Task DeleteAsync(int id, string actorUserId)
    {
        var a = await _db.Assignments.FindAsync(id);
        if (a is null) return;
        a.Deleted = true;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "Assignment", id, "Deleted assignment");
    }

    public async Task UpdateStatusAsync(int id, AssignmentStatus status, string actorUserId)
    {
        var a = await _db.Assignments.FindAsync(id);
        if (a is null) return;
        a.Status = status;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Assignment", id, $"Status changed to {status}");
    }

    private async Task<bool> HasOverlapAsync(int personId, DateTime start, DateTime end, int? excludeId = null)
    {
        var query = _db.Assignments.Where(a =>
            a.PersonId == personId &&
            a.Status != AssignmentStatus.Cancelado &&
            a.Status != AssignmentStatus.Rechazado &&
            a.StartDateTime < end && a.EndDateTime > start);

        if (excludeId.HasValue)
            query = query.Where(a => a.AssignmentId != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> IsValidAssignment(int personId, DateTime start, DateTime end, int? excludeId = null)
    {
        if (await HasOverlapAsync(personId, start, end, excludeId)) return false;
        var person = await _db.Persons.Include(p => p.Availabilities).FirstOrDefaultAsync(p => p.PersonId == personId);
        if (person is null) return false;
        return _availability.IsPersonAvailable(person, start, end);
    }
}
