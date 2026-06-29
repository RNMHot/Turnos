using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Turnos.Services;

public class TurnosUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
{
    private readonly AccessControlService _accessControl;
    private readonly ILogger<TurnosUserClaimsPrincipalFactory> _logger;

    public TurnosUserClaimsPrincipalFactory(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor,
        AccessControlService accessControl,
        ILogger<TurnosUserClaimsPrincipalFactory> logger)
        : base(userManager, roleManager, optionsAccessor)
    {
        _accessControl = accessControl;
        _logger = logger;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        try
        {
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Could not build app-specific claims for user {Email}. Defaulting to no access.",
                user.Email);

            identity.AddClaim(new Claim(TurnosClaimTypes.AppAccess, "false"));
            identity.AddClaim(new Claim(TurnosClaimTypes.CheckInOnly, "false"));
        }

        return identity;
    }
}
