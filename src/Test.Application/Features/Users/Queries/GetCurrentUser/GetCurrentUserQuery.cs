using Test.Application.Features.Users.DTOs;

namespace Test.Application.Features.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<UserDto>;
