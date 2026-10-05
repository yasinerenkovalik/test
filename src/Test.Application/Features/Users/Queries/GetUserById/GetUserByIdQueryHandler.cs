using Test.Application.Features.Users.DTOs;

namespace Test.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(
    IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        // Kullanıcı sadece kendi kaydını görebilir; Admin herkesi görebilir.
        if (!currentUser.IsAdmin && currentUser.UserId != request.Id)
            throw new ForbiddenException();

        return await db.Users.AsNoTracking()
                   .Where(u => u.Id == request.Id)
                   .ProjectTo<UserDto>(mapper.ConfigurationProvider)
                   .FirstOrDefaultAsync(ct)
               ?? throw new NotFoundException(nameof(User), request.Id);
    }
}
