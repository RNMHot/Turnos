using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class Event
{
    public int EventId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente")]
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    [Required, MaxLength(300)]
    public string EventName { get; set; } = string.Empty;

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    [Range(0, int.MaxValue)]
    public int RequiredSupervisors { get; set; }

    [Range(0, int.MaxValue)]
    public int RequiredUshers { get; set; }

    [Range(0, int.MaxValue)]
    public int RequiredOther { get; set; }

    [Required, MaxLength(100)]
    public string RequiredOtherLabel { get; set; } = "Other";

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    public string? Notes { get; set; }

    public bool Active { get; set; } = true;

    public bool Deleted { get; set; }

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<MessageLog> MessageLogs { get; set; } = new List<MessageLog>();
}
