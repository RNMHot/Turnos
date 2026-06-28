using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Turnos.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(SignInManager<IdentityUser> signInManager)
        => _signInManager = signInManager;

    [HttpPost("signin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(
        string email, string password, bool rememberMe = false, string? returnUrl = null)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
            return Redirect(returnUrl ?? "/");

        return Redirect("/account/login?error=1&returnUrl=" + Uri.EscapeDataString(returnUrl ?? ""));
    }

    [HttpGet("signout")]
    public async Task<IActionResult> SignOut()
    {
        await _signInManager.SignOutAsync();
        return Redirect("/account/login");
    }
}
