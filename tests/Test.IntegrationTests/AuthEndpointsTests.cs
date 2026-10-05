using Test.Application.Features.Auth.DTOs;
using Test.Application.Features.Users.DTOs;

namespace Test.IntegrationTests;

[Collection(ApiCollection.Name)]
public class AuthEndpointsTests(ApiFactory factory)
{
    private static string NewEmail() => $"user-{Guid.NewGuid():N}@example.com";
    private const string Password = "Passw0rdX";

    private Task<HttpResponseMessage> RegisterAsync(string email, string password = Password)
        => factory.CreateClient().PostJsonAsync("/api/auth/register",
            new { firstName = "Ada", lastName = "Lovelace", email, password });

    [Fact]
    public async Task Register_WithValidData_Returns201AndTokens()
    {
        var response = await RegisterAsync(NewEmail());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadApiAsync<AuthResponse>();
        body.Success.Should().BeTrue();
        body.StatusCode.Should().Be(201);
        body.TraceId.Should().NotBeNullOrWhiteSpace();
        body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.Data.User.Role.Should().Be(Test.Domain.Enums.UserRole.User);
    }

    [Fact]
    public async Task Register_WithInvalidData_Returns400WithFieldErrors()
    {
        var response = await RegisterAsync("not-an-email", "weak");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.ReadApiAsync<object>();
        body.Success.Should().BeFalse();
        body.Message.Should().Be("Validation failed.");
        body.Errors.Select(e => e.Field).Should().Contain(["Email", "Password"]);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns422()
    {
        var email = NewEmail();
        (await RegisterAsync(email)).StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await RegisterAsync(email);

        second.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        var email = NewEmail();
        await RegisterAsync(email);

        var response = await factory.CreateClient().PostJsonAsync("/api/auth/login", new { email, password = Password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadApiAsync<AuthResponse>();
        body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Data.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var email = NewEmail();
        await RegisterAsync(email);

        var response = await factory.CreateClient().PostJsonAsync("/api/auth/login", new { email, password = "WrongPass1" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await response.ReadApiAsync<object>()).Success.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshToken_RotatesAndRejectsOldToken()
    {
        var email = NewEmail();
        var registered = (await (await RegisterAsync(email)).ReadApiAsync<AuthResponse>()).Data!;

        var refreshed = await factory.CreateClient().PostJsonAsync("/api/auth/refresh-token",
            new { refreshToken = registered.RefreshToken });
        refreshed.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokens = (await refreshed.ReadApiAsync<AuthResponse>()).Data!;
        newTokens.RefreshToken.Should().NotBe(registered.RefreshToken);

        var reuse = await factory.CreateClient().PostJsonAsync("/api/auth/refresh-token",
            new { refreshToken = registered.RefreshToken });
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_WithToken_ReturnsCurrentUser_AndWithoutToken_Returns401()
    {
        var email = NewEmail();
        var auth = (await (await RegisterAsync(email)).ReadApiAsync<AuthResponse>()).Data!;

        var ok = await factory.CreateClient().WithBearer(auth.AccessToken).GetAsync("/api/users/me");
        ok.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ok.ReadApiAsync<UserDto>()).Data!.Email.Should().Be(email);

        var unauthorized = await factory.CreateClient().GetAsync("/api/users/me");
        unauthorized.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await unauthorized.ReadApiAsync<object>()).Success.Should().BeFalse();
    }
}
