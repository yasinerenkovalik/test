using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Test.Application.Common.Interfaces;
using Test.Infrastructure.Identity;
using Test.Infrastructure.Persistence;
using Test.Infrastructure.Persistence.Interceptors;

namespace Test.UnitTests.Helpers;

public sealed class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
    public bool IsAdmin { get; set; }
}

/// <summary>PBKDF2 yavaş olduğu için unit testlerde sahte hasher kullanılır.</summary>
public sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => "hashed:" + password;
    public bool Verify(string passwordHash, string password) => passwordHash == "hashed:" + password;
}

public static class TestSetup
{
    // Her test kendi izole InMemory veritabanını alır; gerçek interceptor (audit/soft delete) da devrededir.
    public static ApplicationDbContext CreateDb(ICurrentUserService? currentUser = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new AuditableEntityInterceptor(currentUser ?? new FakeCurrentUserService()))
            .Options;
        return new ApplicationDbContext(options);
    }

    public static IMapper CreateMapper()
        => new MapperConfiguration(cfg => cfg.AddMaps(typeof(Test.Application.DependencyInjection).Assembly))
            .CreateMapper();

    public static ITokenService CreateTokenService()
        => new TokenService(new JwtSettings
        {
            Secret = new string('x', 64),
            Issuer = "test-issuer",
            Audience = "test-audience"
        });
}
