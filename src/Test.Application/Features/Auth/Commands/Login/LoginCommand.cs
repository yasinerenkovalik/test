using Test.Application.Features.Auth.DTOs;
using Test.Application.Features.Auth.Services;

namespace Test.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
