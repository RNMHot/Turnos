using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class PersonService
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public PersonService(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<List<Person>> GetAllAsync(string? search = null, int? roleId = null, bool? active = null)
    {
        var query = _db.Persons.Include(p => p.PersonRoles).ThenInclude(pr => pr.Role).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.FullName.Contains(search) || p.PhoneNumber.Contains(search));
        if (roleId.HasValue)
            query = query.Where(p => p.PersonRoles.Any(pr => pr.RoleId == roleId));
        if (active.HasValue)
            query = query.Where(p => p.Active == active.Value);

        return await query.OrderBy(p => p.FullName).ToListAsync();
    }

    public async Task<Person?> GetByIdAsync(int id) =>
        await _db.Persons
            .Include(p => p.PersonRoles).ThenInclude(pr => pr.Role)
            .Include(p => p.Availabilities)
            .FirstOrDefaultAsync(p => p.PersonId == id);

    public async Task<List<Person>> GetAllWithAvailabilityAsync(bool? active = null)
    {
        var query = _db.Persons
            .Include(p => p.PersonRoles).ThenInclude(pr => pr.Role)
            .Include(p => p.Availabilities)
            .AsQueryable();

        if (active.HasValue)
            query = query.Where(p => p.Active == active.Value);

        return await query.OrderBy(p => p.FullName).ToListAsync();
    }

    public async Task<Person> CreateAsync(Person person, List<int> roleIds, string actorUserId)
    {
        _db.Persons.Add(person);
        await _db.SaveChangesAsync();

        foreach (var rid in roleIds)
            _db.PersonRoles.Add(new PersonRole { PersonId = person.PersonId, RoleId = rid });
        await _db.SaveChangesAsync();

        await _audit.LogAsync(actorUserId, "Create", "Person", person.PersonId, $"Created {person.FullName}");
        return person;
    }

    public async Task UpdateAsync(Person person, List<int> roleIds, string actorUserId)
    {
        var existing = await _db.PersonRoles
            .IgnoreQueryFilters()
            .Where(pr => pr.PersonId == person.PersonId)
            .ToListAsync();
        _db.PersonRoles.RemoveRange(existing);

        foreach (var rid in roleIds)
            _db.PersonRoles.Add(new PersonRole { PersonId = person.PersonId, RoleId = rid });

        _db.Persons.Update(person);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(actorUserId, "Update", "Person", person.PersonId, $"Updated {person.FullName}");
    }

    public async Task SoftDeleteAsync(int id, string actorUserId)
    {
        var person = await _db.Persons.FindAsync(id);
        if (person is null) return;
        person.Active = false;
        person.Deleted = true;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "Person", id, $"Deleted {person.FullName}");
    }

    public async Task<List<Role>> GetRolesAsync() => await _db.StaffRoles.ToListAsync();
}
