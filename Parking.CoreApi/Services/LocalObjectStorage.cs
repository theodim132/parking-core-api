namespace Parking.CoreApi.Services;

public sealed class LocalObjectStorage : IObjectStorage
{
    private readonly StorageOptions _options;

    public LocalObjectStorage(StorageOptions options)
    {
        _options = options;
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct)
    {
        Directory.CreateDirectory(_options.RootPath);
        var safeName = Path.GetFileName(fileName);
        var key = $"{Guid.NewGuid()}_{safeName}";
        var path = Path.Combine(_options.RootPath, key);

        await using var file = File.Create(path);
        await content.CopyToAsync(file, ct);
        return key;
    }

    public Task<Stream> DownloadAsync(string storageKey, CancellationToken ct)
    {
        var path = Path.Combine(_options.RootPath, storageKey);
        Stream stream = File.OpenRead(path);
        return Task.FromResult(stream);
    }
}
