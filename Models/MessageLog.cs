using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class MessageLog
{
    [Key]
    public int MessageId { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public int? EventId { get; set; }
    public Event? Event { get; set; }

    [MaxLength(100)]
    public string MessageType { get; set; } = string.Empty;

    public string MessageBody { get; set; } = string.Empty;

    public DateTime SentDateTime { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string DeliveryStatus { get; set; } = "Pending";
}
