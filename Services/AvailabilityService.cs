using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class AvailabilityService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AvailabilityService(IDbContextFactory<AppDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<List<Availability>> GetForPersonAsync(int personId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Availabilities.Where(a => a.PersonId == personId).ToListAsync();
    }

    public async Task<Availability> CreateAsync(Availability availability)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Availabilities.Add(availability);
        await db.SaveChangesAsync();
        return availability;
    }

    public async Task UpdateAsync(Availability availability)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Availabilities.Update(availability);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var a = await db.Availabilities.FindAsync(id);
        if (a is null) return;
        a.Deleted = true;
        await db.SaveChangesAsync();
    }

    private static DateTime ToLocal(DateTime dt) => dt.Kind == DateTimeKind.Utc ? dt.ToLocalTime() : dt;

    public bool IsPersonAvailable(Person person, DateTime start, DateTime end)
    {
        var localStart = ToLocal(start);
        var localEnd = ToLocal(end);
        var dayOfWeek = localStart.DayOfWeek.ToString();
        var startTime = localStart.TimeOfDay;
        var endTime = localEnd.TimeOfDay;
        var date = localStart.Date;

        // People are available by default; only explicit unavailability blocks them.
        foreach (var avail in person.Availabilities.Where(a => !a.Deleted && a.IsUnavailable))
        {
            bool dayMatches = avail.AvailabilityType == AvailabilityType.FechaEspecifica
                ? avail.SpecificDate.HasValue && avail.SpecificDate.Value.Date == date
                : avail.DaysOfWeek != null && avail.DaysOfWeek.Contains(dayOfWeek);

            if (!dayMatches) continue;

            if (avail.StartTime < endTime && avail.EndTime > startTime)
                return false;
        }

        return true;
    }
}
