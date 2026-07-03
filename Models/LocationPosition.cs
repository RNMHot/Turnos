using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class LocationPosition
{
    public int LocationPositionId { get; set; }

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public bool Deleted { get; set; }

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
