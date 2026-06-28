namespace Turnos.Models;

public enum AssignmentStatus
{
    Open,
    Offered,
    Accepted,
    Declined,
    Assigned,
    Confirmed,
    Completed,
    Cancelled
}

public class Assignment
{
    public int AssignmentId { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Open;

    public string? Notes { get; set; }
}
