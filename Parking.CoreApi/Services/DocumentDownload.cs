namespace Parking.CoreApi.Services;

public sealed record DocumentDownload(Stream Stream, string ContentType, string FileName);