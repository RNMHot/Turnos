using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
                return RedirectAfterAuth("/account/change-password?returnUrl=" + Uri.EscapeDataString(returnUrl ?? ""));

            var roles = await _accessControl.GetRoleNamesAsync(user);
            var hasFullAccess = roles.Contains("Gerencia") || roles.Contains("Admin");

            if (!hasFullAccess)
            {
                var defaultUrl = roles.Contains("Supervisor") ? "/supervisor" : "/attendance/checkin";
                return RedirectAfterAuth(IsUsableReturnUrl(returnUrl) ? returnUrl! : defaultUrl);
            }

            return RedirectAfterAuth(returnUrl ?? "/");
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

        var roles = await _accessControl.GetRoleNamesAsync(user);
        var hasFullAccess = roles.Contains("Gerencia") || roles.Contains("Admin");

        if (!hasFullAccess)
        {
            var defaultUrl = roles.Contains("Supervisor") ? "/supervisor" : "/attendance/checkin";
            return RedirectAfterAuth(IsUsableReturnUrl(returnUrl) ? returnUrl! : defaultUrl);
        }

        return RedirectAfterAuth(returnUrl ?? "/");
    }

    // Safari on iOS can drop a cookie set on a response that also carries a 3xx redirect
    // (observed on some devices right after sign-in). Sending a 200 page that navigates via
    // JS gives the cookie jar a chance to commit before the next request is made.
    private IActionResult RedirectAfterAuth(string url)
    {
        var html = $"<!DOCTYPE html><html><head><meta charset=\"utf-8\"></head>" +
                   $"<body><script>location.replace({JsonSerializer.Serialize(url)});</script></body></html>";
        return Content(html, "text/html");
    }

    // "/" is the Gerencia/Admin-only home page. Blindly honoring a returnUrl of "/" for a
    // non-full-access user sends them straight into an access-denied bounce back to this same
    // login page, looking exactly like a failed sign-in.
    private static bool IsUsableReturnUrl(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && returnUrl != "/";

    [HttpGet("signout")]
    public async Task<IActionResult> SignOutUser()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/account/login");
    }
}
