using Test.Application.Common.Models;

namespace Test.API.Common;

/// <summary>Controller dışındaki yerlerde (exception handler, JWT event'leri) standart zarfı yazar.</summary>
public static class ApiResponseWriter
{
    public static Task WriteAsync(HttpContext context, int statusCode, string message,
        IReadOnlyList<ApiError>? errors = null, CancellationToken cancellationToken = default)
    {
        context.Response.StatusCode = statusCode;
        var body = ApiResponse<object>.Fail(message, statusCode, context.TraceIdentifier, errors);
        return context.Response.WriteAsJsonAsync(body, cancellationToken);
    }
}
