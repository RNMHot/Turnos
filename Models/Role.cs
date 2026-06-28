using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class Role
{
    public int RoleId { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<PersonRole> PersonRoles { get; set; } = new List<PersonRole>();
}
