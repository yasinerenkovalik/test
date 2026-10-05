using Test.Application.Features.Users.DTOs;

namespace Test.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;
