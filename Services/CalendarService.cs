using System.Security.Cryptography;
using System.Text;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class CalendarService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public CalendarService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public string GenerateToken(int eventId, int personId)
    {
        var secret = _config["CalendarToken:Secret"] ?? "turnos-default-secret-key-2024";
        var raw = $"{eventId}:{personId}:{secret}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return $"{eventId}-{personId}-{Convert.ToBase64String(hash)[..16].Replace("/", "_").Replace("+", "-")}";
    }

    public (int EventId, int PersonId)? ParseToken(string token)
    {
        var parts = token.Split('-');
        if (parts.Length < 3 || !int.TryParse(parts[0], out var eventId) || !int.TryParse(parts[1], out var personId))
            return null;

        var expected = GenerateToken(eventId, personId);
        return token == expected ? (eventId, personId) : null;
    }

    public async Task<string?> GenerateIcsContentAsync(int eventId, int personId)
    {
        var ev = await _db.Events.Include(e => e.Company).FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null) return null;

        var calendar = new Ical.Net.Calendar();
        var calEvent = new CalendarEvent
        {
            Summary = $"{ev.EventName} — {ev.Company.Name}",
            Location = ev.Location?.Name ?? string.Empty,
            Description = ev.Notes ?? string.Empty,
            Start = new CalDateTime(ev.StartDateTime, "UTC"),
            End = new CalDateTime(ev.EndDateTime, "UTC"),
            Uid = $"event-{eventId}-person-{personId}@turnos"
        };

        calendar.Events.Add(calEvent);

        var serializer = new CalendarSerializer();
        return serializer.SerializeToString(calendar);
    }

    public string GenerateIcsUrl(int eventId, int personId, string baseUrl)
    {
        var token = GenerateToken(eventId, personId);
        return $"{baseUrl.TrimEnd('/')}/calendar/invite/{token}";
    }
}
