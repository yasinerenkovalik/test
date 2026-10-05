using Test.Application.Common.Validation;
using Test.Application.Features.Auth.DTOs;
using Test.Application.Features.Auth.Services;
using Test.Domain.Enums;

namespace Test.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler(
    IApplicationDbContext db, IPasswordHasher hasher, AuthTokenIssuer issuer)
    : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email, ct))
            throw new BusinessException("A user with this email already exists.");

        // Public register her zaman normal User üretir; Admin sadece seed/yönetimle oluşur.
        var user = User.Create(
            request.FirstName.Trim(), request.LastName.Trim(), email,
            hasher.Hash(request.Password), request.PhoneNumber?.Trim(), UserRole.User);

        db.Users.Add(user);
        var (response, _) = issuer.Issue(user);
        await db.SaveChangesAsync(ct);

        return response;
    }
}
