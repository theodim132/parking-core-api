using Parking.CoreApi.Dtos;
using Parking.CoreApi.Models;
using Parking.CoreApi.Repositories;

namespace Parking.CoreApi.Services;

public sealed class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _repo;

    public ApplicationService(IApplicationRepository repo)
    {
        _repo = repo;
    }

    public Task<List<ParkingPermitApplication>> GetForCitizenAsync(string citizenId, CancellationToken ct)
    {
        return _repo.GetByCitizenAsync(citizenId, ct);
    }

    public Task<ParkingPermitApplication?> GetByIdForCitizenAsync(Guid id, string citizenId, CancellationToken ct)
    {
        return _repo.GetByIdForCitizenAsync(id, citizenId, ct);
    }

    public async Task<ParkingPermitApplication> CreateAsync(string citizenId, CreateApplicationRequest request, CancellationToken ct)
    {
        var app = new ParkingPermitApplication
        {
            CitizenId = citizenId,
            FullName = request.FullName,
            Address = request.Address,
            PlateNumber = request.PlateNumber,
            Email = request.Email,
            Phone = request.Phone,
            Status = ApplicationStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _repo.AddApplication(app);
        await _repo.SaveChangesAsync(ct);
        return app;
    }

    public async Task<ServiceResult> UpdateAsync(Guid id, string citizenId, UpdateApplicationRequest request, CancellationToken ct)
    {
        var app = await _repo.GetByIdForCitizenAsync(id, citizenId, ct);
        if (app is null)
        {
            return ServiceResult.Fail("NotFound", "Application not found.");
        }

        if (app.Status != ApplicationStatus.Draft)
        {
            return ServiceResult.Fail("InvalidState", "Only draft applications can be updated.");
        }

        app.FullName = request.FullName;
        app.Address = request.Address;
        app.PlateNumber = request.PlateNumber;
        app.Email = request.Email;
        app.Phone = request.Phone;
        app.UpdatedAt = DateTimeOffset.UtcNow;

        await _repo.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SubmitAsync(Guid id, string citizenId, CancellationToken ct)
    {
        var app = await _repo.GetByIdForCitizenAsync(id, citizenId, ct);
        if (app is null)
        {
            return ServiceResult.Fail("NotFound", "Application not found.");
        }

        if (app.Status != ApplicationStatus.Draft)
        {
            return ServiceResult.Fail("InvalidState", "Only draft applications can be submitted.");
        }

        app.Status = ApplicationStatus.Submitted;
        app.UpdatedAt = DateTimeOffset.UtcNow;

        _repo.AddOutboxEmail(new OutboxEmail
        {
            ToEmail = app.Email,
            Subject = "Application submitted",
            Body = $"Your parking permit application {app.Id} was submitted.",
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _repo.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }

    public Task<List<ParkingPermitApplication>> GetAdminListAsync(ApplicationStatus? status, CancellationToken ct)
    {
        return _repo.GetByStatusAsync(status, ct);
    }

    public async Task<ServiceResult> DecideAsync(Guid id, AdminDecisionRequest request, CancellationToken ct)
    {
        if (request.Status != ApplicationStatus.Approved && request.Status != ApplicationStatus.Rejected)
        {
            return ServiceResult.Fail("Validation", "Status must be Approved or Rejected.");
        }

        var app = await _repo.GetByIdAsync(id, ct);
        if (app is null)
        {
            return ServiceResult.Fail("NotFound", "Application not found.");
        }

        if (app.Status != ApplicationStatus.Submitted)
        {
            return ServiceResult.Fail("InvalidState", "Only submitted applications can be decided.");
        }

        app.Status = request.Status;
        app.UpdatedAt = DateTimeOffset.UtcNow;

        var decisionText = request.Status == ApplicationStatus.Approved ? "approved" : "rejected";
        var reason = string.IsNullOrWhiteSpace(request.Reason) ? string.Empty : $" Reason: {request.Reason}";

        _repo.AddOutboxEmail(new OutboxEmail
        {
            ToEmail = app.Email,
            Subject = $"Application {decisionText}",
            Body = $"Your parking permit application {app.Id} was {decisionText}.{reason}",
            Status = "Pending",
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _repo.SaveChangesAsync(ct);
        return ServiceResult.Ok();
    }
}