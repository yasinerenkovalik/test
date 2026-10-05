using Microsoft.Extensions.Logging;

namespace Test.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUserService currentUser)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName} (User: {UserId}) {Request}",
            name, currentUser.UserId, SensitiveDataSanitizer.ToSafeJson(request));

        var response = await next();

        logger.LogInformation("Handled {RequestName}", name);
        return response;
    }
}
