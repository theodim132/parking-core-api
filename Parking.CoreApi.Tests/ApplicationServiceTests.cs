using Parking.CoreApi.Dtos;
using Parking.CoreApi.Models;
using Parking.CoreApi.Repositories;
using Parking.CoreApi.Services;

namespace Parking.CoreApi.Tests;

public sealed class ApplicationServiceTests
{
    [Fact]
    public async Task CreateAsync_CreatesDraftApplication()
    {
        var repo = new FakeRepository();
        var service = new ApplicationService(repo);

        var request = new CreateApplicationRequest
        {
            FullName = "Demo Citizen",
            Address = "Main 1",
            PlateNumber = "ABC1234",
            Email = "citizen@local",
            Phone = "6900000000"
        };

        var app = await service.CreateAsync("citizen1", request, CancellationToken.None);

        Assert.Equal("citizen1", app.CitizenId);
        Assert.Equal(ApplicationStatus.Draft, app.Status);
        Assert.Single(repo.Applications);
    }

    [Fact]
    public async Task SubmitAsync_AddsOutboxEmail()
    {
        var repo = new FakeRepository();
        var app = new ParkingPermitApplication
        {
            CitizenId = "citizen1",
            FullName = "Demo Citizen",
            Address = "Main 1",
            PlateNumber = "ABC1234",
            Email = "citizen@local",
            Phone = "6900000000",
            Status = ApplicationStatus.Draft
        };
        repo.Applications.Add(app);

        var service = new ApplicationService(repo);

        var result = await service.SubmitAsync(app.Id, "citizen1", CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(ApplicationStatus.Submitted, app.Status);
        Assert.Single(repo.Outbox);
    }

    private sealed class FakeRepository : IApplicationRepository
    {
        public List<ParkingPermitApplication> Applications { get; } = new();
        public List<ApplicationDocument> Documents { get; } = new();
        public List<OutboxEmail> Outbox { get; } = new();

        public Task<List<ParkingPermitApplication>> GetByCitizenAsync(string citizenId, CancellationToken ct)
        {
            var apps = Applications.Where(a => a.CitizenId == citizenId).ToList();
            return Task.FromResult(apps);
        }

        public Task<ParkingPermitApplication?> GetByIdForCitizenAsync(Guid id, string citizenId, CancellationToken ct)
        {
            var app = Applications.FirstOrDefault(a => a.Id == id && a.CitizenId == citizenId);
            return Task.FromResult(app);
        }

        public Task<ParkingPermitApplication?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var app = Applications.FirstOrDefault(a => a.Id == id);
            return Task.FromResult(app);
        }

        public Task<List<ParkingPermitApplication>> GetByStatusAsync(ApplicationStatus? status, CancellationToken ct)
        {
            var apps = Applications.Where(a => !status.HasValue || a.Status == status.Value).ToList();
            return Task.FromResult(apps);
        }

        public Task<ApplicationDocument?> GetDocumentByIdAsync(Guid id, CancellationToken ct)
        {
            var doc = Documents.FirstOrDefault(d => d.Id == id);
            return Task.FromResult(doc);
        }

        public void AddApplication(ParkingPermitApplication app)
        {
            Applications.Add(app);
        }

        public void AddDocument(ApplicationDocument doc)
        {
            Documents.Add(doc);
        }

        public void AddOutboxEmail(OutboxEmail email)
        {
            Outbox.Add(email);
        }

        public Task SaveChangesAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}