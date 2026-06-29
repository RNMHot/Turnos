using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class Person
{
    public int PersonId { get; set; }

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Email { get; set; }

    public bool Active { get; set; } = true;

    public bool CheckInOnly { get; set; }

    public bool Deleted { get; set; }

    public string? Notes { get; set; }

    public ICollection<PersonRole> PersonRoles { get; set; } = new List<PersonRole>();
    public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<MessageLog> MessageLogs { get; set; } = new List<MessageLog>();
}
