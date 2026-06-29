using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Turnos.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly Turnos.Services.AccessControlService _accessControl;

    public AccountController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        Turnos.Services.AccessControlService accessControl)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _accessControl = accessControl;
    }

    [HttpPost("signin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(
        string email, string password, bool rememberMe = false, string? returnUrl = null)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null || !await _accessControl.HasAppAccessAsync(user))
            {
                await _signInManager.SignOutAsync();
                return Redirect("/account/login?denied=1&returnUrl=" + Uri.EscapeDataString(returnUrl ?? ""));
            }

            if (await _accessControl.IsCheckInOnlyUserAsync(user))
                return Redirect("/attendance/checkin");

            return Redirect(returnUrl ?? "/");
        }

        return Redirect("/account/login?error=1&returnUrl=" + Uri.EscapeDataString(returnUrl ?? ""));
    }

    [HttpGet("signout")]
    public async Task<IActionResult> SignOutUser()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/account/login");
    }
}
