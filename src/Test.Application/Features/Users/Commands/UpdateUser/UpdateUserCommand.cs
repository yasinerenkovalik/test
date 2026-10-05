using Test.Application.Features.Users.DTOs;

namespace Test.Application.Features.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(string FirstName, string LastName, string? PhoneNumber = null)
    : IRequest<UserDto>;
