using Parking.CoreApi.Dtos;
using Parking.CoreApi.Models;

namespace Parking.CoreApi.Services;

public interface IApplicationService
{
    Task<List<ParkingPermitApplication>> GetForCitizenAsync(string citizenId, CancellationToken ct);
    Task<ParkingPermitApplication?> GetByIdForCitizenAsync(Guid id, string citizenId, CancellationToken ct);
    Task<ParkingPermitApplication> CreateAsync(string citizenId, CreateApplicationRequest request, CancellationToken ct);
    Task<ServiceResult> UpdateAsync(Guid id, string citizenId, UpdateApplicationRequest request, CancellationToken ct);
    Task<ServiceResult> SubmitAsync(Guid id, string citizenId, CancellationToken ct);
    Task<List<ParkingPermitApplication>> GetAdminListAsync(ApplicationStatus? status, CancellationToken ct);
    Task<ServiceResult> DecideAsync(Guid id, AdminDecisionRequest request, CancellationToken ct);
}