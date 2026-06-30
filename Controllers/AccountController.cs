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

            if (await _accessControl.MustChangePasswordAsync(user))
                return Redirect("/account/change-password?returnUrl=" + Uri.EscapeDataString(returnUrl ?? ""));

            if (await _accessControl.IsCheckInOnlyUserAsync(user))
                return Redirect("/attendance/checkin");

            return Redirect(returnUrl ?? "/");
        }

        return Redirect("/account/login?error=1&returnUrl=" + Uri.EscapeDataString(returnUrl ?? ""));
    }

    [HttpPost("change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        string newPassword, string confirmPassword, string? returnUrl = null)
    {
        if (newPassword != confirmPassword)
            return Redirect("/account/change-password?error=mismatch&returnUrl=" + Uri.EscapeDataString(returnUrl ?? ""));

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Redirect("/account/login");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return Redirect("/account/change-password?error=invalid&returnUrl=" + Uri.EscapeDataString(returnUrl ?? ""));

        await _accessControl.ClearMustChangePasswordAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        if (await _accessControl.IsCheckInOnlyUserAsync(user))
            return Redirect("/attendance/checkin");

        return Redirect(returnUrl ?? "/");
    }

    [HttpGet("signout")]
    public async Task<IActionResult> SignOutUser()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/account/login");
    }
}
