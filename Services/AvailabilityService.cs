using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class AvailabilityService
{
    private readonly AppDbContext _db;

    public AvailabilityService(AppDbContext db) => _db = db;

    public async Task<List<Availability>> GetForPersonAsync(int personId) =>
        await _db.Availabilities.Where(a => a.PersonId == personId).ToListAsync();

    public async Task<Availability> CreateAsync(Availability availability)
    {
        _db.Availabilities.Add(availability);
        await _db.SaveChangesAsync();
        return availability;
    }

    public async Task UpdateAsync(Availability availability)
    {
        _db.Availabilities.Update(availability);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var a = await _db.Availabilities.FindAsync(id);
        if (a is null) return;
        a.Deleted = true;
        await _db.SaveChangesAsync();
    }

    public bool IsPersonAvailable(Person person, DateTime start, DateTime end)
    {
        var dayOfWeek = start.DayOfWeek.ToString();
        var startTime = start.TimeOfDay;
        var endTime = end.TimeOfDay;
        var date = start.Date;

        foreach (var avail in person.Availabilities)
        {
            if (avail.IsUnavailable)
            {
                if (avail.AvailabilityType == AvailabilityType.SpecificDate &&
                    avail.SpecificDate.HasValue && avail.SpecificDate.Value.Date == date)
                    return false;
                if (avail.AvailabilityType == AvailabilityType.Recurring &&
                    avail.DaysOfWeek != null && avail.DaysOfWeek.Contains(dayOfWeek))
                    return false;
                continue;
            }

            if (avail.AvailabilityType == AvailabilityType.SpecificDate)
            {
                if (avail.SpecificDate.HasValue && avail.SpecificDate.Value.Date == date &&
                    avail.StartTime <= startTime && avail.EndTime >= endTime)
                    return true;
            }
            else if (avail.AvailabilityType == AvailabilityType.Recurring)
            {
                if (avail.DaysOfWeek != null && avail.DaysOfWeek.Contains(dayOfWeek) &&
                    avail.StartTime <= startTime && avail.EndTime >= endTime)
                    return true;
            }
        }

        return false;
    }
}
