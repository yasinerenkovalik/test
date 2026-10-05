using Test.Application.Common.Validation;
using Test.Application.Features.Auth.DTOs;
using Test.Application.Features.Auth.Services;
using Test.Domain.Enums;

namespace Test.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber = null) : IRequest<AuthResponse>;
