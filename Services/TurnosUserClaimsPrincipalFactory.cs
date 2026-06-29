using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Turnos.Services;

public class TurnosUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
{
    private readonly AccessControlService _accessControl;

    public TurnosUserClaimsPrincipalFactory(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        AccessControlService accessControl)
        : base(userManager, roleManager, optionsAccessor)
    {
        _accessControl = accessControl;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var hasAccess = await _accessControl.HasAppAccessAsync(user);
        identity.AddClaim(new Claim(TurnosClaimTypes.AppAccess, hasAccess ? "true" : "false"));

        var personId = await _accessControl.GetPersonIdAsync(user);
        if (personId.HasValue)
            identity.AddClaim(new Claim(TurnosClaimTypes.PersonId, personId.Value.ToString()));

        var roles = await _accessControl.GetRoleNamesAsync(user);
        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        var isCheckInOnly = await _accessControl.IsCheckInOnlyUserAsync(user);
        identity.AddClaim(new Claim(TurnosClaimTypes.CheckInOnly, isCheckInOnly ? "true" : "false"));

        return identity;
    }
}
