using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Turnos.Models;
using Turnos.Services;

namespace Turnos.Components.Pages.Events;

public partial class EventForm
{
    [Parameter] public int Id { get; set; }

    private Event ev = new()
    {
        Active = true,
        StartDateTime = RoundDownToHourUtc(DateTime.UtcNow),
        EndDateTime = RoundDownToHourUtc(DateTime.UtcNow).AddHours(4),
        RequiredOtherLabel = "Otros"
    };

    private List<Company> companies = new();
    private List<Location> locations = new();
    private bool saving;
    private string? recurrenceError;
    private string userId = string.Empty;
    private DateTime startLocal;
    private DateTime endLocal;
    private readonly List<RecurrenceRuleEditor> recurrenceRules = new();

    protected override async Task OnInitializedAsync()
    {
        var auth = await AuthState.GetAuthenticationStateAsync();
        userId = auth.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        companies = await CompanySvc.GetAllAsync();
        locations = await LocationSvc.GetAllAsync();

        if (Id != 0)
        {
            var existing = await EventSvc.GetByIdAsync(Id);
            if (existing is not null)
                ev = existing;
        }

        if (string.IsNullOrWhiteSpace(ev.RequiredOtherLabel))
            ev.RequiredOtherLabel = "Otros";

        if (Id == 0 && recurrenceRules.Count == 0)
            recurrenceRules.Add(CreateDefaultRule());

        startLocal = ToLocalInputTime(ev.StartDateTime);
        endLocal = ToLocalInputTime(ev.EndDateTime);
    }

    private async Task Save()
    {
        saving = true;
        try
        {
            if (Id == 0 && ev.IsRecurring)
            {
                var occurrences = BuildOccurrences();
                if (!occurrences.Any())
                {
                    recurrenceError = "No se generaron ocurrencias. Verifica que las reglas tengan fechas válidas y, para días de la semana, que hayas seleccionado al menos un día.";
                    return;
                }
                recurrenceError = null;

                foreach (var occurrence in occurrences)
                {
                    await EventSvc.CreateAsync(occurrence, userId);
                }
            }
            else
            {
                ev.StartDateTime = DateTime.SpecifyKind(startLocal, DateTimeKind.Local).ToUniversalTime();
                ev.EndDateTime = DateTime.SpecifyKind(endLocal, DateTimeKind.Local).ToUniversalTime();

                if (Id == 0)
                    await EventSvc.CreateAsync(ev, userId);
                else
                    await EventSvc.UpdateAsync(ev, userId);
            }

            Nav.NavigateTo("/events");
        }
        finally
        {
            saving = false;
        }
    }

    private static DateTime ToLocalInputTime(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value.ToLocalTime(),
            DateTimeKind.Local => value,
            _ => DateTime.SpecifyKind(value, DateTimeKind.Local)
        };
    }

    private void OnStartChanged()
    {
        if (Id != 0)
            return;

        endLocal = startLocal.AddHours(4);

        if (!ev.IsRecurring && recurrenceRules.Count > 0)
        {
            recurrenceRules[0].StartTimeText = startLocal.ToString("HH:mm");
            recurrenceRules[0].EndTimeText = endLocal.ToString("HH:mm");
            recurrenceRules[0].SingleDate = startLocal.Date;
        }
    }

    private void AddRecurrenceRule() => recurrenceRules.Add(CreateDefaultRule());

    private void RemoveRecurrenceRule(RecurrenceRuleEditor rule)
    {
        if (recurrenceRules.Count > 1)
            recurrenceRules.Remove(rule);
    }

    private void ToggleRuleDay(RecurrenceRuleEditor rule, DayOfWeek day, bool selected)
    {
        if (selected)
        {
            if (!rule.Days.Contains(day))
                rule.Days.Add(day);
        }
        else
        {
            rule.Days.Remove(day);
        }
    }

    private static void SetRuleKind(RecurrenceRuleEditor rule, string? value)
    {
        if (Enum.TryParse<RecurrenceRuleKind>(value, out var kind))
            rule.Kind = kind;
    }

    private static void SetRuleDate(RecurrenceRuleEditor rule, string? value)
    {
        if (DateTime.TryParse(value, out var parsed))
            rule.SingleDate = parsed.Date;
    }

    private static void SetRuleRangeStart(RecurrenceRuleEditor rule, string? value)
    {
        if (DateTime.TryParse(value, out var parsed))
            rule.RangeStartDate = parsed.Date;
    }

    private static void SetRuleRangeEnd(RecurrenceRuleEditor rule, string? value)
    {
        if (DateTime.TryParse(value, out var parsed))
            rule.RangeEndDate = parsed.Date;
    }

    private static void SetRuleSpecificDates(RecurrenceRuleEditor rule, string? value) =>
        rule.SpecificDatesText = value ?? string.Empty;

    private List<Event> BuildOccurrences()
    {
        var occurrences = new List<Event>();
        foreach (var rule in recurrenceRules)
        {
            foreach (var occurrence in BuildOccurrences(rule))
                occurrences.Add(occurrence);
        }

        return occurrences;
    }

    private IEnumerable<Event> BuildOccurrences(RecurrenceRuleEditor rule)
    {
        var dates = rule.Kind switch
        {
            RecurrenceRuleKind.SingleDate => new[] { rule.SingleDate.Date },
            RecurrenceRuleKind.WeekdaysInRange => BuildWeekdayDates(rule),
            RecurrenceRuleKind.SpecificDates => ParseSpecificDates(rule.SpecificDatesText),
            _ => Enumerable.Empty<DateTime>()
        };

        if (!TryParseTime(rule.StartTimeText, out var startTime) || !TryParseTime(rule.EndTimeText, out var endTime))
            yield break;

        foreach (var date in dates.Distinct().OrderBy(d => d))
        {
            var start = date.Date.Add(startTime);
            var end = date.Date.Add(endTime);
            if (end <= start)
                end = end.AddDays(1);

            yield return new Event
            {
                CompanyId = ev.CompanyId,
                EventName = ev.EventName,
                StartDateTime = DateTime.SpecifyKind(start, DateTimeKind.Local).ToUniversalTime(),
                EndDateTime = DateTime.SpecifyKind(end, DateTimeKind.Local).ToUniversalTime(),
                RequiredSupervisors = ev.RequiredSupervisors,
                RequiredUshers = ev.RequiredUshers,
                RequiredOther = ev.RequiredOther,
                RequiredOtherLabel = ev.RequiredOtherLabel,
                LocationId = ev.LocationId,
                Notes = ev.Notes,
                Active = ev.Active,
                IsRecurring = true,
                RecurrencePattern = DescribeRule(rule)
            };
        }
    }

    private IEnumerable<DateTime> BuildWeekdayDates(RecurrenceRuleEditor rule)
    {
        var from = rule.RangeStartDate.Date;
        var to = rule.RangeEndDate.Date;

        if (to < from)
            (from, to) = (to, from);

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            if (rule.Days.Contains(date.DayOfWeek))
                yield return date;
        }
    }

    private static IEnumerable<DateTime> ParseSpecificDates(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        var parts = text.Split(new[] { ',', '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            if (DateTime.TryParse(part, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                yield return parsed.Date;
            else if (DateTime.TryParse(part, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
                yield return parsed.Date;
        }
    }

    private static bool TryParseTime(string? text, out TimeSpan time)
    {
        if (TimeSpan.TryParse(text, out time))
            return true;

        time = default;
        return false;
    }

    private static string FormatDateInput(DateTime value) => value.ToString("yyyy-MM-dd");

    private static string DescribeRule(RecurrenceRuleEditor rule)
    {
        return rule.Kind switch
        {
            RecurrenceRuleKind.SingleDate => $"SingleDate: {rule.SingleDate:yyyy-MM-dd} {rule.StartTimeText}-{rule.EndTimeText}",
            RecurrenceRuleKind.WeekdaysInRange => $"WeekdaysInRange: {rule.RangeStartDate:yyyy-MM-dd}..{rule.RangeEndDate:yyyy-MM-dd} [{string.Join(',', rule.Days)}] {rule.StartTimeText}-{rule.EndTimeText}",
            RecurrenceRuleKind.SpecificDates => $"SpecificDates: {rule.SpecificDatesText.Replace('\r', ' ').Replace('\n', ' ')} {rule.StartTimeText}-{rule.EndTimeText}",
            _ => string.Empty
        };
    }

    private static RecurrenceRuleEditor CreateDefaultRule() => new()
    {
        Kind = RecurrenceRuleKind.SingleDate,
        SingleDate = DateTime.Today,
        RangeStartDate = DateTime.Today,
        RangeEndDate = DateTime.Today.AddDays(30),
        StartTimeText = "09:00",
        EndTimeText = "17:00"
    };

    // Days ordered Monday-first (Spanish/Latin American convention)
    private static readonly DayOfWeek[] DaysOrdered =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    private static string DayName(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday    => "Lun",
        DayOfWeek.Tuesday   => "Mar",
        DayOfWeek.Wednesday => "Mié",
        DayOfWeek.Thursday  => "Jue",
        DayOfWeek.Friday    => "Vie",
        DayOfWeek.Saturday  => "Sáb",
        DayOfWeek.Sunday    => "Dom",
        _ => day.ToString()
    };

    private int PreviewCount => ev.IsRecurring ? BuildOccurrences().Count : 0;

    private static DateTime RoundDownToHourUtc(DateTime value)
    {
        var utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return new DateTime(utcValue.Year, utcValue.Month, utcValue.Day, utcValue.Hour, 0, 0, DateTimeKind.Utc);
    }

    private enum RecurrenceRuleKind
    {
        SingleDate,
        WeekdaysInRange,
        SpecificDates
    }

    private sealed class RecurrenceRuleEditor
    {
        public RecurrenceRuleKind Kind { get; set; }
        public DateTime SingleDate { get; set; }
        public DateTime RangeStartDate { get; set; }
        public DateTime RangeEndDate { get; set; }
        public string StartTimeText { get; set; } = "09:00";
        public string EndTimeText { get; set; } = "17:00";
        public string SpecificDatesText { get; set; } = string.Empty;
        public List<DayOfWeek> Days { get; } = new();
    }
}
