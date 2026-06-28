using Microsoft.AspNetCore.Mvc;
using Turnos.Services;

namespace Turnos.Controllers;

[Route("calendar")]
public class CalendarController : Controller
{
    private readonly CalendarService _calendar;

    public CalendarController(CalendarService calendar) => _calendar = calendar;

    [HttpGet("invite/{token}")]
    public async Task<IActionResult> GetInvite(string token)
    {
        var parsed = _calendar.ParseToken(token);
        if (parsed is null) return NotFound();

        var ics = await _calendar.GenerateIcsContentAsync(parsed.Value.EventId, parsed.Value.PersonId);
        if (ics is null) return NotFound();

        return File(System.Text.Encoding.UTF8.GetBytes(ics), "text/calendar", "invite.ics");
    }
}
