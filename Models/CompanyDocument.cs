using System.ComponentModel.DataAnnotations;

namespace Turnos.Models;

public class CompanyDocument
{
    public int CompanyDocumentId { get; set; }

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public byte[] Data { get; set; } = Array.Empty<byte>();

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
