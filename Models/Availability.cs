namespace Turnos.Models;

public enum AvailabilityType
{
    Recurring,
    SpecificDate
}

public class Availability
{
    public int AvailabilityId { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public AvailabilityType AvailabilityType { get; set; }

    public string? DaysOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public DateTime? SpecificDate { get; set; }

    public bool IsUnavailable { get; set; } = false;

    public bool Deleted { get; set; }
}
