using Test.Application.Common.Validation;
using Test.Application.Features.Auth.DTOs;
using Test.Application.Features.Auth.Services;
using Test.Domain.Enums;

namespace Test.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).MustBeStrongPassword();
        RuleFor(x => x.PhoneNumber).Matches(@"^\+?[0-9\s\-]{7,20}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Phone number is not valid.");
    }
}
