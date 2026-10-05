using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Test.Application.Common.Interfaces;
using Test.Application.Common.Models;
using Test.Application.Features.Auth.DTOs;
using Test.Domain.Entities;
using Test.Domain.Enums;
using Test.Infrastructure.Persistence;
using Test.Infrastructure.Persistence.Interceptors;

namespace Test.IntegrationTests;

/// <summary>
/// Gerçek pipeline (auth, validation, exception handler) çalışır; sadece PostgreSQL yerine InMemory kullanılır.
/// Docker gerektirmez, CI'da hızlıdır.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@test.local";
    public const string AdminPassword = "Admin!2345Test";

    private readonly string _dbName = $"integration-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Secret", new string('k', 64));
        builder.UseSetting("Jwt:Issuer", "Test.API");
        builder.UseSetting("Jwt:Audience", "Test.Client");

        builder.ConfigureServices(services =>
        {
            services.AddDbContext<ApplicationDbContext>((sp, options) => options
                .UseInMemoryDatabase(_dbName)
                .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddHostedService<AdminSeedHostedService>();
        });
    }

    private sealed class AdminSeedHostedService(IServiceScopeFactory scopes) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            db.Users.Add(User.Create("Admin", "User", AdminEmail, hasher.Hash(AdminPassword), null, UserRole.Admin));
            await db.SaveChangesAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

// Tek host, testler sırayla: paylaşılan InMemory DB + Serilog dosya sink çakışması olmaz.
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}

internal static class TestJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}

internal static class ApiTestExtensions
{
    public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string url, T body)
        => client.PostAsJsonAsync(url, body, TestJson.Options);

    public static async Task<ApiResponse<T>> ReadApiAsync<T>(this HttpResponseMessage response)
        => (await response.Content.ReadFromJsonAsync<ApiResponse<T>>(TestJson.Options))!;

    public static HttpClient WithBearer(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public static async Task<AuthResponse> LoginAsync(this ApiFactory factory, string email, string password)
    {
        var response = await factory.CreateClient().PostJsonAsync("/api/auth/login", new { email, password });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadApiAsync<AuthResponse>()).Data!;
    }

    public static async Task<HttpClient> CreateAdminClientAsync(this ApiFactory factory)
    {
        var auth = await factory.LoginAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);
        return factory.CreateClient().WithBearer(auth.AccessToken);
    }
}
