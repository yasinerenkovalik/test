using Test.Application.Common.Validation;

namespace Test.Application.Features.Users.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    IApplicationDbContext db, ICurrentUserService currentUser, IPasswordHasher hasher)
    : IRequestHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException(nameof(User), userId);

        if (!hasher.Verify(user.PasswordHash, request.CurrentPassword))
            throw new BusinessException("Current password is incorrect.");

        user.SetPasswordHash(hasher.Hash(request.NewPassword));

        // Şifre değişince açık tüm oturumlar (refresh token'lar) kapatılır.
        var now = DateTime.UtcNow;
        var tokens = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAtUtc == null)
            .ToListAsync(ct);
        foreach (var token in tokens) token.Revoke(now);

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
