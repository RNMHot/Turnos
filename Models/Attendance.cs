using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class Attendance
{
    public int AttendanceId { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public DateTime CheckInDateTime { get; set; }
    public DateTime? CheckOutDateTime { get; set; }

    [MaxLength(400)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool Deleted { get; set; }

    public ICollection<AttendanceBreak> Breaks { get; set; } = new List<AttendanceBreak>();
}

public class AttendanceBreak
{
    public int AttendanceBreakId { get; set; }

    public int AttendanceId { get; set; }
    public Attendance Attendance { get; set; } = null!;

    public DateTime BreakStartDateTime { get; set; }
    public DateTime? BreakEndDateTime { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }

    public bool Deleted { get; set; }
}
