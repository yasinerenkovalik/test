using Test.Application.Common.Exceptions;
using Test.Application.Features.Auth.Commands.Login;
using Test.Application.Features.Auth.Commands.RefreshToken;
using Test.Application.Features.Auth.Commands.Register;
using Test.Application.Features.Auth.Services;
using Test.Domain.Enums;
using Test.Infrastructure.Persistence;

namespace Test.UnitTests.Handlers;

public class AuthHandlerTests
{
    private readonly ApplicationDbContext _db = TestSetup.CreateDb();
    private readonly Application.Common.Interfaces.ITokenService _tokens = TestSetup.CreateTokenService();
    private readonly FakePasswordHasher _hasher = new();

    private AuthTokenIssuer Issuer() => new(_tokens, _db, TestSetup.CreateMapper());
    private RegisterCommandHandler RegisterHandler() => new(_db, _hasher, Issuer());
    private LoginCommandHandler LoginHandler() => new(_db, _hasher, Issuer());
    private RefreshTokenCommandHandler RefreshHandler() => new(_db, _tokens, Issuer());

    [Fact]
    public async Task Register_ValidRequest_CreatesUserWithHashedPasswordAndStoresOnlyTokenHash()
    {
        var response = await RegisterHandler().Handle(
            new RegisterCommand("Ada", "Lovelace", "Ada@Example.com", "Passw0rdX"), CancellationToken.None);

        var user = await _db.Users.SingleAsync();
        user.Email.Should().Be("ada@example.com");
        user.PasswordHash.Should().NotBe("Passw0rdX").And.StartWith("hashed:");
        user.Role.Should().Be(UserRole.User);
        user.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5)); // interceptor

        response.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.User.Email.Should().Be("ada@example.com");

        var stored = await _db.RefreshTokens.SingleAsync();
        stored.TokenHash.Should().NotBe(response.RefreshToken); // ham token saklanmaz
        stored.TokenHash.Should().Be(_tokens.HashToken(response.RefreshToken));
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsBusinessException()
    {
        var command = new RegisterCommand("Ada", "Lovelace", "ada@example.com", "Passw0rdX");
        await RegisterHandler().Handle(command, CancellationToken.None);

        Func<Task> act = () => RegisterHandler().Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        await RegisterHandler().Handle(
            new RegisterCommand("Ada", "Lovelace", "ada@example.com", "Passw0rdX"), CancellationToken.None);

        var response = await LoginHandler().Handle(new LoginCommand("ADA@example.com", "Passw0rdX"), CancellationToken.None);

        response.AccessToken.Should().NotBeNullOrWhiteSpace();
        response.RefreshToken.Should().NotBeNullOrWhiteSpace();
        response.Expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorized()
    {
        await RegisterHandler().Handle(
            new RegisterCommand("Ada", "Lovelace", "ada@example.com", "Passw0rdX"), CancellationToken.None);

        Func<Task> act = () => LoginHandler().Handle(new LoginCommand("ada@example.com", "WrongPass1"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Login_UnknownUser_ThrowsUnauthorized()
    {
        Func<Task> act = () => LoginHandler().Handle(new LoginCommand("nobody@example.com", "Passw0rdX"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task RefreshToken_Valid_RotatesToken()
    {
        var first = await RegisterHandler().Handle(
            new RegisterCommand("Ada", "Lovelace", "ada@example.com", "Passw0rdX"), CancellationToken.None);

        var second = await RefreshHandler().Handle(new RefreshTokenCommand(first.RefreshToken), CancellationToken.None);

        second.RefreshToken.Should().NotBe(first.RefreshToken);

        var oldRecord = await _db.RefreshTokens.SingleAsync(t => t.TokenHash == _tokens.HashToken(first.RefreshToken));
        oldRecord.RevokedAtUtc.Should().NotBeNull();
        oldRecord.ReplacedByTokenHash.Should().Be(_tokens.HashToken(second.RefreshToken));
    }

    [Fact]
    public async Task RefreshToken_ReusedToken_RevokesAllActiveTokens()
    {
        var first = await RegisterHandler().Handle(
            new RegisterCommand("Ada", "Lovelace", "ada@example.com", "Passw0rdX"), CancellationToken.None);
        var second = await RefreshHandler().Handle(new RefreshTokenCommand(first.RefreshToken), CancellationToken.None);

        // Eski (kullanılmış) token tekrar denenir -> reddedilmeli ve yeni token da iptal edilmeli.
        Func<Task> act = () => RefreshHandler().Handle(new RefreshTokenCommand(first.RefreshToken), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedException>();

        var newRecord = await _db.RefreshTokens.SingleAsync(t => t.TokenHash == _tokens.HashToken(second.RefreshToken));
        newRecord.RevokedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task RefreshToken_UnknownToken_ThrowsUnauthorized()
    {
        Func<Task> act = () => RefreshHandler().Handle(new RefreshTokenCommand("does-not-exist"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
