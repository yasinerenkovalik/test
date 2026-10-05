using Test.Domain.Enums;

namespace Test.Application.Features.Users.DTOs;

public sealed record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    UserRole Role,
    bool IsActive,
    DateTime CreatedDate);
