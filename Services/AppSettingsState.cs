namespace Turnos.Services;

public class AppSettingsState
{
    public const double DefaultTimeZoneOffsetHours = -4;
    public static readonly DateTime DefaultBiweeklyPeriodStartDate = new(2026, 1, 7);

    public string? BrandLogoBase64 { get; private set; }
    public double TimeZoneOffsetHours { get; private set; } = DefaultTimeZoneOffsetHours;
    public DateTime BiweeklyPeriodStartDate { get; private set; } = DefaultBiweeklyPeriodStartDate;

    public event Action? OnChange;

    public void SetBrandLogo(string? value)
    {
        BrandLogoBase64 = value;
        OnChange?.Invoke();
    }

    public void SetTimeZoneOffsetHours(double hours)
    {
        TimeZoneOffsetHours = hours;
        OnChange?.Invoke();
    }

    public void SetBiweeklyPeriodStartDate(DateTime date)
    {
        BiweeklyPeriodStartDate = date.Date;
        OnChange?.Invoke();
    }

    // Returns the start of the 14-day biweekly period that contains the given date,
    // anchored to BiweeklyPeriodStartDate (periods recur every 14 days from that date).
    public DateTime GetBiweeklyPeriodStart(DateTime date)
    {
        var daysSinceAnchor = (date.Date - BiweeklyPeriodStartDate).Days;
        var periodsElapsed = (int)Math.Floor(daysSinceAnchor / 14.0);
        return BiweeklyPeriodStartDate.AddDays(periodsElapsed * 14);
    }

    public DateTime ToDisplay(DateTime utc) =>
        DateTime.SpecifyKind(utc.AddHours(TimeZoneOffsetHours), DateTimeKind.Unspecified);

    public DateTime ToUtc(DateTime display) =>
        DateTime.SpecifyKind(display.AddHours(-TimeZoneOffsetHours), DateTimeKind.Utc);

    public DateTime Now => ToDisplay(DateTime.UtcNow);
    public DateTime Today => Now.Date;
}
