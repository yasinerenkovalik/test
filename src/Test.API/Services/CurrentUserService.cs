using System.Security.Claims;
using Test.Application.Common.Interfaces;
using Test.Application.Common.Security;

namespace Test.API.Services;

public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public Guid? UserId => Guid.TryParse(Principal?.FindFirstValue("sub"), out var id) ? id : null;
    public string? Email => Principal?.FindFirstValue("email");
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    public bool IsAdmin => Principal?.IsInRole(Roles.Admin) ?? false;
}
