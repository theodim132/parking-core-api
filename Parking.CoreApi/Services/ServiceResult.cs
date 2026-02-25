namespace Parking.CoreApi.Services;

public sealed record ServiceResult(string? Code, string? Error)
{
    public bool Success => Error is null;

    public static ServiceResult Ok() => new(null, null);

    public static ServiceResult Fail(string code, string error) => new(code, error);
}

public sealed record ServiceResult<T>(T? Value, string? Code, string? Error)
{
    public bool Success => Error is null;

    public static ServiceResult<T> Ok(T value) => new(value, null, null);

    public static ServiceResult<T> Fail(string code, string error) => new(default, code, error);
}