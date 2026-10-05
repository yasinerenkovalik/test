using Test.Application.Features.Auth.DTOs;
using Test.Application.Features.Auth.Services;

namespace Test.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
