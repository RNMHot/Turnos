using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class PersonService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly AuditService _audit;
    private readonly UserManager<IdentityUser> _userManager;

    public PersonService(IDbContextFactory<AppDbContext> dbFactory, AuditService audit, UserManager<IdentityUser> userManager)
    {
        _dbFactory = dbFactory;
        _audit = audit;
        _userManager = userManager;
    }

    public async Task<List<Person>> GetAllAsync(string? search = null, int? roleId = null, bool? active = null, bool? isMassGroup = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Persons.Include(p => p.PersonRoles).ThenInclude(pr => pr.Role).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.FullName.Contains(search) || p.PhoneNumber.Contains(search));
        if (roleId.HasValue)
            query = query.Where(p => p.PersonRoles.Any(pr => pr.RoleId == roleId));
        if (active.HasValue)
            query = query.Where(p => p.Active == active.Value);
        if (isMassGroup.HasValue)
            query = query.Where(p => p.IsMassGroup == isMassGroup.Value);

        return await query.OrderBy(p => p.FullName).ToListAsync();
    }

    public async Task<Person?> GetByIdAsync(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Persons
            .Include(p => p.PersonRoles).ThenInclude(pr => pr.Role)
            .Include(p => p.Availabilities)
            .FirstOrDefaultAsync(p => p.PersonId == id);
    }

    public async Task<List<Person>> GetAllWithAvailabilityAsync(bool? active = null, bool? isMassGroup = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Persons
            .Include(p => p.PersonRoles).ThenInclude(pr => pr.Role)
            .Include(p => p.Availabilities)
            .AsQueryable();

        if (active.HasValue)
            query = query.Where(p => p.Active == active.Value);
        if (isMassGroup.HasValue)
            query = query.Where(p => p.IsMassGroup == isMassGroup.Value);

        return await query.OrderBy(p => p.FullName).ToListAsync();
    }

    public async Task<Person> CreateAsync(Person person, List<int> roleIds, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Persons.Add(person);
        await db.SaveChangesAsync();

        foreach (var rid in roleIds)
            db.PersonRoles.Add(new PersonRole { PersonId = person.PersonId, RoleId = rid });
        await db.SaveChangesAsync();

        await _audit.LogAsync(actorUserId, "Create", "Person", person.PersonId, $"Created {person.FullName}");
        return person;
    }

    public async Task UpdateAsync(Person person, List<int> roleIds, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.PersonRoles
            .IgnoreQueryFilters()
            .Where(pr => pr.PersonId == person.PersonId)
            .ToListAsync();
        db.PersonRoles.RemoveRange(existing);

        foreach (var rid in roleIds)
            db.PersonRoles.Add(new PersonRole { PersonId = person.PersonId, RoleId = rid });

        db.Entry(person).State = EntityState.Modified;
        await db.SaveChangesAsync();

        await _audit.LogAsync(actorUserId, "Update", "Person", person.PersonId, $"Updated {person.FullName}");
    }

    public async Task SoftDeleteAsync(int id, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var person = await db.Persons.FindAsync(id);
        if (person is null) return;
        person.Active = false;
        person.Deleted = true;
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "Person", id, $"Deleted {person.FullName}");
    }

    public async Task<List<Role>> GetRolesAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.StaffRoles.OrderBy(l => l.Name).ToListAsync();
    }

    public async Task<(bool Success, string Error)> SetPasswordAsync(int personId, string newPassword, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var person = await db.Persons.AsNoTracking().FirstOrDefaultAsync(p => p.PersonId == personId);
        if (person is null) return (false, "Persona no encontrada.");
        if (string.IsNullOrEmpty(person.Email)) return (false, "La persona no tiene email configurado.");

        var user = await _userManager.FindByEmailAsync(person.Email);
        IdentityResult result;
        if (user is null)
        {
            user = new IdentityUser { UserName = person.Email, Email = person.Email, EmailConfirmed = true };
            result = await _userManager.CreateAsync(user, newPassword);
        }
        else
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        if (!result.Succeeded)
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));

        await _audit.LogAsync(actorUserId, "SetPassword", "Person", personId, $"Password set for {person.FullName}");
        return (true, "");
    }
}
