using Test.Domain.Enums;

namespace Test.Application.Features.Auth.DTOs;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime Expiration,
    DateTime RefreshTokenExpiration,
    AuthUserDto User);
