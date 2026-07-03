using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class Role
{
    public int RoleId { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    public int Rank { get; set; }

    public bool Deleted { get; set; }

    public ICollection<PersonRole> PersonRoles { get; set; } = new List<PersonRole>();
}
