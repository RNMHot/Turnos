namespace Turnos.Models;

public class PersonRole
{
    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public bool Deleted { get; set; }
}
