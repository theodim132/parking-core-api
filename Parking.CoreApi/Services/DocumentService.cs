using Parking.CoreApi.Dtos;
using Parking.CoreApi.Models;
using Parking.CoreApi.Repositories;

namespace Parking.CoreApi.Services;

public sealed class DocumentService : IDocumentService
{
    private readonly IApplicationRepository _repo;
    private readonly IObjectStorage _storage;

    public DocumentService(IApplicationRepository repo, IObjectStorage storage)
    {
        _repo = repo;
        _storage = storage;
    }

    public async Task<ServiceResult<DocumentUploadResponse>> UploadAsync(Guid applicationId, string citizenId, IFormFile file, CancellationToken ct)
    {
        if (file.Length <= 0)
        {
            return ServiceResult<DocumentUploadResponse>.Fail("Validation", "Empty file.");
        }

        var app = await _repo.GetByIdForCitizenAsync(applicationId, citizenId, ct);
        if (app is null)
        {
            return ServiceResult<DocumentUploadResponse>.Fail("NotFound", "Application not found.");
        }

        if (app.Status != ApplicationStatus.Draft)
        {
            return ServiceResult<DocumentUploadResponse>.Fail("InvalidState", "Documents can only be uploaded for draft applications.");
        }

        await using var stream = file.OpenReadStream();
        var key = await _storage.UploadAsync(stream, file.FileName, file.ContentType, ct);

        var doc = new ApplicationDocument
        {
            ApplicationId = app.Id,
            FileName = file.FileName,
            ContentType = file.ContentType ?? "application/octet-stream",
            StorageKey = key,
            SizeBytes = file.Length,
            UploadedAt = DateTimeOffset.UtcNow
        };

        _repo.AddDocument(doc);
        await _repo.SaveChangesAsync(ct);

        return ServiceResult<DocumentUploadResponse>.Ok(new DocumentUploadResponse
        {
            Id = doc.Id,
            FileName = doc.FileName,
            StorageKey = doc.StorageKey
        });
    }

    public async Task<ServiceResult<DocumentDownload>> DownloadAsync(Guid documentId, CancellationToken ct)
    {
        var doc = await _repo.GetDocumentByIdAsync(documentId, ct);
        if (doc is null)
        {
            return ServiceResult<DocumentDownload>.Fail("NotFound", "Document not found.");
        }

        var stream = await _storage.DownloadAsync(doc.StorageKey, ct);
        var contentType = string.IsNullOrWhiteSpace(doc.ContentType) ? "application/octet-stream" : doc.ContentType;
        return ServiceResult<DocumentDownload>.Ok(new DocumentDownload(stream, contentType, doc.FileName));
    }
}