using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace Parking.CoreApi.Services;

public sealed class S3ObjectStorage : IObjectStorage
{
    private readonly StorageOptions _options;
    private readonly IAmazonS3 _client;

    public S3ObjectStorage(StorageOptions options)
    {
        _options = options;
        var credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
        var config = new AmazonS3Config
        {
            ServiceURL = _options.Endpoint,
            ForcePathStyle = _options.ForcePathStyle
        };
        _client = new AmazonS3Client(credentials, config);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, CancellationToken ct)
    {
        await EnsureBucketExistsAsync(ct);
        var safeName = Path.GetFileName(fileName);
        var key = $"{Guid.NewGuid()}_{safeName}";

        var request = new PutObjectRequest
        {
            BucketName = _options.Bucket,
            Key = key,
            InputStream = content,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
        };

        await _client.PutObjectAsync(request, ct);
        return key;
    }

    public async Task<Stream> DownloadAsync(string storageKey, CancellationToken ct)
    {
        using var response = await _client.GetObjectAsync(_options.Bucket, storageKey, ct);
        var memory = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memory, ct);
        memory.Position = 0;
        return memory;
    }

    private async Task EnsureBucketExistsAsync(CancellationToken ct)
    {
        if (await AmazonS3Util.DoesS3BucketExistV2Async(_client, _options.Bucket))
        {
            return;
        }

        var request = new PutBucketRequest
        {
            BucketName = _options.Bucket
        };
        await _client.PutBucketAsync(request, ct);
    }
}
