namespace Test.Application.Common.Validation;

public static class PasswordRules
{
    /// <summary>Register ve ChangePassword için tek tanım (DRY).</summary>
    public static IRuleBuilderOptions<T, string> MustBeStrongPassword<T>(this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
}
