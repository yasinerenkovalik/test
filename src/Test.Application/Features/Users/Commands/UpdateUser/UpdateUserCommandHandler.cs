using Test.Application.Features.Users.DTOs;

namespace Test.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler(
    IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    : IRequestHandler<UpdateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new NotFoundException(nameof(User), userId);

        user.UpdateProfile(request.FirstName.Trim(), request.LastName.Trim(), request.PhoneNumber?.Trim());
        await db.SaveChangesAsync(ct);

        return mapper.Map<UserDto>(user);
    }
}
