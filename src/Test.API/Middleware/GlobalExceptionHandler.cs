using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Test.API.Common;
using Test.Application.Common.Exceptions;
using Test.Application.Common.Models;
using Test.Domain.Exceptions;

namespace Test.API.Middleware;

/// <summary>
/// .NET'in IExceptionHandler altyapısı: controller'larda try/catch yok.
/// Her exception uygun HTTP status + ApiResponse'a dönüşür; stack trace dışarı verilmez.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var errors = new List<ApiError>();
        int status;
        string message;

        switch (exception)
        {
            case ValidationException validation:
                status = StatusCodes.Status400BadRequest;
                message = "Validation failed.";
                errors.AddRange(validation.Errors.Select(e => new ApiError(e.PropertyName, e.ErrorMessage)));
                logger.LogWarning("Validation failed: {Errors}", errors.Count);
                break;
            case NotFoundException notFound:
                status = StatusCodes.Status404NotFound;
                message = notFound.Message;
                logger.LogWarning("Not found: {Message}", notFound.Message);
                break;
            case UnauthorizedException unauthorized:
                status = StatusCodes.Status401Unauthorized;
                message = unauthorized.Message;
                logger.LogWarning("Unauthorized: {Message}", unauthorized.Message);
                break;
            case ForbiddenException forbidden:
                status = StatusCodes.Status403Forbidden;
                message = forbidden.Message;
                logger.LogWarning("Forbidden: {Message}", forbidden.Message);
                break;
            case BusinessException business:
                status = StatusCodes.Status422UnprocessableEntity;
                message = business.Message;
                logger.LogWarning("Business rule violation: {Message}", business.Message);
                break;
            default:
                status = StatusCodes.Status500InternalServerError;
                message = "An unexpected error occurred.";
                // Detay sadece loglara; Development'ta yalnızca exception mesajı döner (stack trace asla).
                logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
                if (env.IsDevelopment()) errors.Add(new ApiError(null, exception.Message));
                break;
        }

        await ApiResponseWriter.WriteAsync(context, status, message, errors, ct);
        return true;
    }
}
