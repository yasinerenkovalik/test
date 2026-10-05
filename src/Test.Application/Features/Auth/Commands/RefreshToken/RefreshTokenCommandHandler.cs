using Test.Application.Features.Auth.DTOs;
using Test.Application.Features.Auth.Services;

namespace Test.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IApplicationDbContext db, ITokenService tokens, AuthTokenIssuer issuer)
    : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = tokens.HashToken(request.RefreshToken);
        var stored = await db.RefreshTokens.Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (stored is null)
            throw new UnauthorizedException("Invalid refresh token.");

        var now = DateTime.UtcNow;

        // Daha önce kullanılmış (iptal edilmiş) token tekrar geldi: çalınmış olabilir.
        // Kullanıcının tüm aktif oturumlarını kapat.
        if (stored.RevokedAtUtc is not null)
        {
            var active = await db.RefreshTokens
                .Where(t => t.UserId == stored.UserId && t.RevokedAtUtc == null)
                .ToListAsync(ct);
            foreach (var token in active) token.Revoke(now);
            await db.SaveChangesAsync(ct);

            throw new UnauthorizedException("Refresh token has been revoked.");
        }

        if (stored.ExpiresAtUtc <= now)
            throw new UnauthorizedException("Refresh token has expired.");

        if (!stored.User.IsActive)
            throw new UnauthorizedException("User account is disabled.");

        // Rotation: eski token iptal, yenisi üretilir.
        var (response, newToken) = issuer.Issue(stored.User);
        stored.Revoke(now, newToken.TokenHash);
        await db.SaveChangesAsync(ct);

        return response;
    }
}
