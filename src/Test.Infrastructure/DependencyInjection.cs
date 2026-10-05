using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Test.Application.Common.Interfaces;
using Test.Infrastructure.Identity;
using Test.Infrastructure.Persistence;
using Test.Infrastructure.Persistence.Interceptors;

namespace Test.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Veritabanından bağımsız servisler (güvenlik, audit).</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(JwtSettings.FromConfiguration(configuration));
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<AuditableEntityInterceptor>();
        return services;
    }

    /// <summary>PostgreSQL DbContext kaydı. Integration testler bunu kendi provider'ı ile değiştirir.</summary>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured. " +
                "Use 'dotnet user-secrets' or the ConnectionStrings__DefaultConnection environment variable.");

        services.AddDbContext<ApplicationDbContext>((sp, options) => options
            .UseNpgsql(connectionString)
            .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        return services;
    }
}
