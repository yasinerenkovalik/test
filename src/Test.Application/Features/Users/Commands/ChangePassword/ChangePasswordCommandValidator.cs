using Test.Application.Common.Validation;

namespace Test.Application.Features.Users.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).MustBeStrongPassword()
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from the current password.");
    }
}
