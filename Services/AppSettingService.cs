using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class AppSettingService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AppSettingService(IDbContextFactory<AppDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<string?> GetValueAsync(string key)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var setting = await db.AppSettings.FindAsync(key);
        return setting?.Value;
    }

    public async Task SetValueAsync(string key, string? value)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var setting = await db.AppSettings.FindAsync(key);
        if (setting is null)
        {
            db.AppSettings.Add(new AppSetting { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
            db.AppSettings.Update(setting);
        }
        await db.SaveChangesAsync();
    }
}
