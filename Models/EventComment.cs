using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class EventComment
{
    public int EventCommentId { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    [Required, MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool Deleted { get; set; }
}
