using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class Company
{
    public int CompanyId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ContactName { get; set; }

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    public string? Notes { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
