using MediatR;
using Microsoft.AspNetCore.Mvc;
using Test.Application.Common.Models;

namespace Test.API.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase(ISender sender) : ControllerBase
{
    protected ISender Sender => sender;

    /// <summary>Başarılı sonucu standart ApiResponse zarfı içinde döner.</summary>
    protected IActionResult Success<T>(T data, string message = "Operation completed successfully.",
        int statusCode = StatusCodes.Status200OK)
        => StatusCode(statusCode, ApiResponse<T>.Ok(data, message, statusCode, HttpContext.TraceIdentifier));
}
