namespace Parking.CoreApi.Services;

public interface IObjectStorage
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct);
    Task<Stream> DownloadAsync(string storageKey, CancellationToken ct);
}