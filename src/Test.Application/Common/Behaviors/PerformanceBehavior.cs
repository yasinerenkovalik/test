using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Test.Application.Common.Behaviors;

/// <summary>Eşik değerden uzun süren request'leri uyarı olarak loglar.</summary>
public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUser)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private const int ThresholdMilliseconds = 500;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > ThresholdMilliseconds)
        {
            logger.LogWarning("Long running request: {RequestName} took {ElapsedMs} ms (User: {UserId})",
                typeof(TRequest).Name, stopwatch.ElapsedMilliseconds, currentUser.UserId);
        }

        return response;
    }
}
