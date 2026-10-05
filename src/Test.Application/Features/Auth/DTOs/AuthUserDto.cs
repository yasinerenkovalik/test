using Test.Domain.Enums;

namespace Test.Application.Features.Auth.DTOs;

public sealed record AuthUserDto(Guid Id, string Email, string FirstName, string LastName, UserRole Role);
