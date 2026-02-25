using Microsoft.EntityFrameworkCore;
using Parking.CoreApi.Data;
using Parking.CoreApi.Models;

namespace Parking.CoreApi.Repositories;

public sealed class ApplicationRepository : IApplicationRepository
{
    private readonly AppDbContext _db;

    public ApplicationRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<ParkingPermitApplication>> GetByCitizenAsync(string citizenId, CancellationToken ct)
    {
        return _db.Applications
            .AsNoTracking()
            .Where(a => a.CitizenId == citizenId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<ParkingPermitApplication?> GetByIdForCitizenAsync(Guid id, string citizenId, CancellationToken ct)
    {
        return _db.Applications
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == id && a.CitizenId == citizenId, ct);
    }

    public Task<ParkingPermitApplication?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return _db.Applications.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public Task<List<ParkingPermitApplication>> GetByStatusAsync(ApplicationStatus? status, CancellationToken ct)
    {
        var query = _db.Applications.AsNoTracking();
        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return query.OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
    }

    public Task<ApplicationDocument?> GetDocumentByIdAsync(Guid id, CancellationToken ct)
    {
        return _db.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public void AddApplication(ParkingPermitApplication app)
    {
        _db.Applications.Add(app);
    }

    public void AddDocument(ApplicationDocument doc)
    {
        _db.Documents.Add(doc);
    }

    public void AddOutboxEmail(OutboxEmail email)
    {
        _db.OutboxEmails.Add(email);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _db.SaveChangesAsync(ct);
    }
}