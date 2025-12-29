using Parking.CoreApi.Dtos;

namespace Parking.CoreApi.Services;

public interface IDocumentService
{
    Task<ServiceResult<DocumentUploadResponse>> UploadAsync(Guid applicationId, string citizenId, IFormFile file, CancellationToken ct);
    Task<ServiceResult<DocumentDownload>> DownloadAsync(Guid documentId, CancellationToken ct);
}