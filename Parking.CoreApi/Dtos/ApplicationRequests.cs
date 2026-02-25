using System.ComponentModel.DataAnnotations;

namespace Parking.CoreApi.Dtos;

public sealed class CreateApplicationRequest
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string PlateNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;
}

public sealed class UpdateApplicationRequest
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string PlateNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Phone { get; set; } = string.Empty;
}