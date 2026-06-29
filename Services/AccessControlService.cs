using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Turnos.Data;

namespace Turnos.Services;

public class AccessControlService
{
    private readonly AppDbContext _db;

    public AccessControlService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasAppAccessAsync(IdentityUser user)
    {
        if (string.Equals(user.Email, TurnosClaimTypes.AdminEmail, StringComparison.OrdinalIgnoreCase))
            return true;

        var person = await GetPersonForUserAsync(user);
        return person?.Active == true;
    }

    public async Task<int?> GetPersonIdAsync(IdentityUser user)
    {
        var person = await GetPersonForUserAsync(user);
        return person?.PersonId;
    }

    public async Task<List<string>> GetRoleNamesAsync(IdentityUser user)
    {
        var roles = new List<string>();

        var person = await GetPersonForUserAsync(user);

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

        var person = await GetPersonForUserAsync(user);
        return person?.Active == true && person.CheckInOnly;
    }

    private async Task<Models.Person?> GetPersonForUserAsync(IdentityUser user)
    {
        return await _db.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Email != null && user.Email != null && p.Email.ToLower() == user.Email.ToLower());
    }
}
