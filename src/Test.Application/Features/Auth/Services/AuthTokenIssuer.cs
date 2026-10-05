using Test.Application.Features.Auth.DTOs;

namespace Test.Application.Features.Auth.Services;

/// <summary>
/// Register/Login/Refresh'in ortak ihtiyacı: access + refresh token üretmek.
/// SaveChanges çağırmaz; çağıran handler transaction sınırını belirler.
/// </summary>
public sealed class AuthTokenIssuer(ITokenService tokens, IApplicationDbContext db, IMapper mapper)
{
    public (AuthResponse Response, RefreshToken Entity) Issue(User user)
    {
        var access = tokens.CreateAccessToken(user);
        var rawRefresh = tokens.GenerateRefreshToken();
        var entity = RefreshToken.Create(
            user.Id, tokens.HashToken(rawRefresh), DateTime.UtcNow.Add(tokens.RefreshTokenLifetime));

        db.RefreshTokens.Add(entity);

        var response = new AuthResponse(
            access.Token, rawRefresh, access.ExpiresAtUtc, entity.ExpiresAtUtc, mapper.Map<AuthUserDto>(user));
        return (response, entity);
    }
}
