using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class Location
{
    public int LocationId { get; set; }

    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? Address { get; set; }

    public string? ParkingInfo { get; set; }

    public bool Deleted { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
