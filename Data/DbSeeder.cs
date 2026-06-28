using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Turnos.Models;

namespace Turnos.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await db.Database.MigrateAsync();

        await SeedRolesAsync(db);
        await SeedAdminUserAsync(userManager);
    }

    private static async Task SeedRolesAsync(AppDbContext db)
    {
        if (await db.StaffRoles.AnyAsync()) return;

        db.StaffRoles.AddRange(
            new Role { Name = "User" },
            new Role { Name = "Usher" },
            new Role { Name = "Supervisor" }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager)
    {
        const string adminEmail = "admin@turnos.local";
        if (await userManager.FindByEmailAsync(adminEmail) is not null) return;

        var admin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(admin, "Admin@123!");
    }
}
