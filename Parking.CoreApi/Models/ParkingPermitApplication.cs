using System.ComponentModel.DataAnnotations;

namespace Parking.CoreApi.Models;

public sealed class ParkingPermitApplication
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(200)]
    public string CitizenId { get; set; } = string.Empty;

    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(30)]
    public string PlateNumber { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Draft;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<ApplicationDocument> Documents { get; set; } = new();
}