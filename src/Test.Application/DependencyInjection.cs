using Microsoft.Extensions.DependencyInjection;
using Test.Application.Common.Behaviors;
using Test.Application.Features.Auth.Services;

namespace Test.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // Kayıt sırası = dıştan içe: Logging -> Performance -> Validation -> Handler
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        // AutoMapper config'i stateless; singleton güvenli.
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(assembly));
        services.AddSingleton(mapperConfig.CreateMapper());

        services.AddScoped<AuthTokenIssuer>();

        return services;
    }
}
