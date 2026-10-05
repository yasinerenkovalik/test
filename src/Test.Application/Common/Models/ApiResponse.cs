namespace Test.Application.Common.Models;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<ApiError> Errors { get; init; } = Array.Empty<ApiError>();
    public int StatusCode { get; init; }
    public string? TraceId { get; init; }

    public static ApiResponse<T> Ok(T? data, string message = "Operation completed successfully.",
        int statusCode = 200, string? traceId = null)
        => new() { Success = true, Data = data, Message = message, StatusCode = statusCode, TraceId = traceId };

    public static ApiResponse<T> Fail(string message, int statusCode, string? traceId = null,
        IReadOnlyList<ApiError>? errors = null)
        => new()
        {
            Success = false,
            Data = default,
            Message = message,
            StatusCode = statusCode,
            TraceId = traceId,
            Errors = errors ?? Array.Empty<ApiError>()
        };
}
