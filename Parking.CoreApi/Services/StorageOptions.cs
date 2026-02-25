namespace Parking.CoreApi.Services;

public sealed class StorageOptions
{
    public string Provider { get; set; } = "Local";
    public string RootPath { get; set; } = "storage";
    public string Endpoint { get; set; } = "http://localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin";
    public string Bucket { get; set; } = "parking-docs";
    public bool ForcePathStyle { get; set; } = true;
}
