using Test.Application.Features.Auth.DTOs;
using Test.Application.Features.Auth.Services;

namespace Test.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IApplicationDbContext db, IPasswordHasher hasher, AuthTokenIssuer issuer)
    : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

        // Kullanıcı yok / şifre yanlış aynı mesajı verir: e-posta enumeration'ı engellenir.
        if (user is null || !hasher.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("User account is disabled.");

        var (response, _) = issuer.Issue(user);
        await db.SaveChangesAsync(ct);

        return response;
    }
}
