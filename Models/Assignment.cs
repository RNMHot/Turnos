namespace Turnos.Models;

public enum AssignmentStatus
{
    Candidato = 0,    // Asignado a un evento durante la planificación; aún no notificado
    Ofrecido = 1,     // Se notificó a la persona la disponibilidad del evento y se le dio la opción de participar
    Aceptado = 2,     // La persona aceptó la oferta de participar en el turno
    Denegado = 3,     // La persona rechazó la oferta de participar en el turno
    Asignado = 4,     // Se fijó a la persona para participar en el turno
    Confirmado = 5    // La persona asignada confirmó tener conocimiento de su asignación
}

public class Assignment
{
    public int AssignmentId { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public int? RoleId { get; set; }
    public Role? Role { get; set; }

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Candidato;

    public int? LocationPositionId { get; set; }
    public LocationPosition? LocationPosition { get; set; }

    public string? Notes { get; set; }

    public bool Deleted { get; set; }
}
