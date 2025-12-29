using System.ComponentModel.DataAnnotations;
using Parking.CoreApi.Models;

namespace Parking.CoreApi.Dtos;

public sealed class AdminDecisionRequest
{
    [Required]
    public ApplicationStatus Status { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}