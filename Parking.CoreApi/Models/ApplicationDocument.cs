using System.ComponentModel.DataAnnotations;

namespace Parking.CoreApi.Models;

public sealed class ApplicationDocument
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ApplicationId { get; set; }

    public ParkingPermitApplication? Application { get; set; }

    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string ContentType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string StorageKey { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
}