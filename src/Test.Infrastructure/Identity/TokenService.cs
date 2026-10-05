using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Test.Application.Common.Interfaces;
using Test.Domain.Entities;

namespace Test.Infrastructure.Identity;

public sealed class TokenService(JwtSettings settings) : ITokenService
{
    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(settings.RefreshTokenDays);

    public AccessToken CreateAccessToken(User user)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("role", user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims, now, expires, credentials);
        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    // 64 byte kriptografik rastgele veri: tahmin edilemez, URL-safe.
    public string GenerateRefreshToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    public string HashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
