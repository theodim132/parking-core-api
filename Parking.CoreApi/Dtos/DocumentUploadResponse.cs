namespace Parking.CoreApi.Dtos;

public sealed class DocumentUploadResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
}