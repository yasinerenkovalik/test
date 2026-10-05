using Test.Application.Common.Validation;

namespace Test.Application.Features.Users.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Unit>;
