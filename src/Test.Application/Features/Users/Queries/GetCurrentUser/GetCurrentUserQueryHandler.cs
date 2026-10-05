using Test.Application.Features.Users.DTOs;

namespace Test.Application.Features.Users.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();

        return await db.Users.AsNoTracking()
                   .Where(u => u.Id == userId)
                   .ProjectTo<UserDto>(mapper.ConfigurationProvider)
                   .FirstOrDefaultAsync(ct)
               ?? throw new NotFoundException(nameof(User), userId);
    }
}
