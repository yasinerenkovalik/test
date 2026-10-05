using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Test.Application.Common.Models;
using Test.Application.Features.Auth.Commands.Login;
using Test.Application.Features.Auth.Commands.RefreshToken;
using Test.Application.Features.Auth.Commands.Register;
using Test.Application.Features.Auth.DTOs;

namespace Test.API.Controllers;

[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Yeni kullanıcı kaydı oluşturur ve token döner.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken ct)
        => Success(await Sender.Send(command, ct), "User registered successfully.", StatusCodes.Status201Created);

    /// <summary>E-posta ve şifre ile giriş yapar.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken ct)
        => Success(await Sender.Send(command, ct), "Login successful.");

    /// <summary>Refresh token ile yeni access/refresh token çifti üretir (rotation).</summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(RefreshTokenCommand command, CancellationToken ct)
        => Success(await Sender.Send(command, ct), "Token refreshed successfully.");
}
