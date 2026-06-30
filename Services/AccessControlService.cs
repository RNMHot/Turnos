using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Turnos.Data;

namespace Turnos.Services;

public class AccessControlService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AccessControlService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<bool> HasAppAccessAsync(IdentityUser user)
    {
        if (string.Equals(user.Email, TurnosClaimTypes.AdminEmail, StringComparison.OrdinalIgnoreCase))
            return true;

        await using var db = await _dbFactory.CreateDbContextAsync();
        var person = await GetPersonForUserAsync(db, user);
        return person?.Active == true;
    }

    public async Task<int?> GetPersonIdAsync(IdentityUser user)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var person = await GetPersonForUserAsync(db, user);
        return person?.PersonId;
    }

    public async Task<List<string>> GetRoleNamesAsync(IdentityUser user)
    {
        var roles = new List<string>();

        await using var db = await _dbFactory.CreateDbContextAsync();
        var person = await GetPersonForUserAsync(db, user);

        if (person?.Active == true)
            roles.Add("User");

        if (string.Equals(user.Email, TurnosClaimTypes.AdminEmail, StringComparison.OrdinalIgnoreCase))
            roles.Add("Admin");

        return roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<bool> IsCheckInOnlyUserAsync(IdentityUser user)
    {
        if (string.Equals(user.Email, TurnosClaimTypes.AdminEmail, StringComparison.OrdinalIgnoreCase))
            return false;

        await using var db = await _dbFactory.CreateDbContextAsync();
        var person = await GetPersonForUserAsync(db, user);
        return person?.Active == true && person.CheckInOnly;
    }

    public async Task<bool> MustChangePasswordAsync(IdentityUser user)
    {
        if (string.Equals(user.Email, TurnosClaimTypes.AdminEmail, StringComparison.OrdinalIgnoreCase))
            return false;

        await using var db = await _dbFactory.CreateDbContextAsync();
        var person = await GetPersonForUserAsync(db, user);
        return person?.MustChangePassword == true;
    }

    public async Task ClearMustChangePasswordAsync(IdentityUser user)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var person = await db.Persons
            .FirstOrDefaultAsync(p => p.Email != null && user.Email != null && p.Email.ToLower() == user.Email.ToLower());
        if (person is not null)
        {
            person.MustChangePassword = false;
            await db.SaveChangesAsync();
        }
    }

    private static async Task<Models.Person?> GetPersonForUserAsync(AppDbContext db, IdentityUser user)
    {
        return await db.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Email != null && user.Email != null && p.Email.ToLower() == user.Email.ToLower());
    }
}
