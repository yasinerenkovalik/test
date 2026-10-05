namespace Test.Application.Common.Interfaces;

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
    TimeSpan RefreshTokenLifetime { get; }
}
