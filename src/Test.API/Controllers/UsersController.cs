using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Test.Application.Common.Models;
using Test.Application.Features.Users.Commands.ChangePassword;
using Test.Application.Features.Users.Commands.UpdateUser;
using Test.Application.Features.Users.DTOs;
using Test.Application.Features.Users.Queries.GetCurrentUser;
using Test.Application.Features.Users.Queries.GetUserById;

namespace Test.API.Controllers;

[Route("api/users")]
[Authorize]
public sealed class UsersController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Kullanıcıyı getirir. Kullanıcı yalnızca kendini, Admin herkesi görebilir.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Success(await Sender.Send(new GetUserByIdQuery(id), ct));

    /// <summary>Giriş yapmış kullanıcının profilini döner.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
        => Success(await Sender.Send(new GetCurrentUserQuery(), ct));

    /// <summary>Giriş yapmış kullanıcının profilini günceller.</summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMe(UpdateUserCommand command, CancellationToken ct)
        => Success(await Sender.Send(command, ct), "Profile updated successfully.");

    /// <summary>Şifreyi değiştirir ve tüm açık oturumları kapatır.</summary>
    [HttpPut("me/change-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken ct)
    {
        await Sender.Send(command, ct);
        return Success<object?>(null, "Password changed successfully.");
    }
}
