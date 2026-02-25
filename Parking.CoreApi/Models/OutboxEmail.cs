using System.ComponentModel.DataAnnotations;

namespace Parking.CoreApi.Models;

public sealed class OutboxEmail
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public string ToEmail { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Status { get; set; } = "Pending";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? SentAt { get; set; }

    public string? Error { get; set; }
}