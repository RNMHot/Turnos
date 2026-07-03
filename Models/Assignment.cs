namespace Turnos.Models;

public enum AssignmentStatus
{
    Disponible = 0,   // Persona respondió que está disponible
    Ofrecido = 1,     // Turno ofrecido, esperando respuesta
    Asignado = 4,     // Adjudicado oficialmente y notificado
    Confirmado = 5,   // Confirmación/recordatorio enviado y reconfirmado
    Candidato = 8     // En consideración; aún no notificado
}

public class Assignment
{
    public int AssignmentId { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Candidato;

    public int? LocationPositionId { get; set; }
    public LocationPosition? LocationPosition { get; set; }

    public string? Notes { get; set; }

    public bool Deleted { get; set; }
}
