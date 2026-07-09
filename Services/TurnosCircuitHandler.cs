using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Turnos.Services;

// Registered as a scoped service so ASP.NET Core creates one instance per Blazor circuit
// (i.e. per connected browser tab). Used to mark a UserSession as connected/disconnected
// in real time, independent of the login/logout events tracked in AccountController.
public class TurnosCircuitHandler : CircuitHandler
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly UserSessionService _sessions;
    private string? _userId;

    public TurnosCircuitHandler(AuthenticationStateProvider authStateProvider, UserSessionService sessions)
    {
        _authStateProvider = authStateProvider;
        _sessions = sessions;
    }

    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _userId = await GetUserIdAsync();
        if (_userId is not null)
            await _sessions.CircuitOpenedAsync(_userId);
    }

    public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (_userId is not null)
            await _sessions.CircuitClosedAsync(_userId);
    }

    private async Task<string?> GetUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
