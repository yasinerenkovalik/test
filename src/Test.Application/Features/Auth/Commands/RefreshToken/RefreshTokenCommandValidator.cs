using Test.Application.Features.Auth.DTOs;
using Test.Application.Features.Auth.Services;

namespace Test.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}
