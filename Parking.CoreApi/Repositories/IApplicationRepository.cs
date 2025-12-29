using Parking.CoreApi.Models;

namespace Parking.CoreApi.Repositories;

public interface IApplicationRepository
{
    Task<List<ParkingPermitApplication>> GetByCitizenAsync(string citizenId, CancellationToken ct);
    Task<ParkingPermitApplication?> GetByIdForCitizenAsync(Guid id, string citizenId, CancellationToken ct);
    Task<ParkingPermitApplication?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<ParkingPermitApplication>> GetByStatusAsync(ApplicationStatus? status, CancellationToken ct);
    Task<ApplicationDocument?> GetDocumentByIdAsync(Guid id, CancellationToken ct);
    void AddApplication(ParkingPermitApplication app);
    void AddDocument(ApplicationDocument doc);
    void AddOutboxEmail(OutboxEmail email);
    Task SaveChangesAsync(CancellationToken ct);
}