using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class UserSession
{
    [Key]
    public int UserSessionId { get; set; }

    [Required, MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? PersonName { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    public DateTime LoginAt { get; set; } = DateTime.UtcNow;

    public DateTime? LogoutAt { get; set; }

    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    // Number of currently open Blazor circuits (browser tabs) for this login.
    public int ConnectedCircuits { get; set; }
}
