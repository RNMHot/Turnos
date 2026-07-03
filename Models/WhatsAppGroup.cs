using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class WhatsAppGroup
{
    [Key]
    public int GroupId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string PhoneNumber { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public bool Active { get; set; } = true;

    public bool Deleted { get; set; }

    public ICollection<MessageLog> MessageLogs { get; set; } = new List<MessageLog>();
}
