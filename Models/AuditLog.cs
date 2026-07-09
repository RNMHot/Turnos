using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Turnos.Models;

public class AuditLog
{
    [Key]
    public int AuditId { get; set; }

    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [NotMapped]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? Details { get; set; }

    public bool Deleted { get; set; }
}
