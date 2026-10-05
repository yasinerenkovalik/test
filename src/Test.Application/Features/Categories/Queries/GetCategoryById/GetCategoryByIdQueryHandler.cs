using Test.Application.Features.Categories.DTOs;

namespace Test.Application.Features.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler(
    IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken ct)
    {
        IQueryable<Category> query = db.Categories.AsNoTracking().Where(c => c.Id == request.Id);
        if (!currentUser.IsAdmin) query = query.Where(c => c.IsActive);

        return await query.ProjectTo<CategoryDto>(mapper.ConfigurationProvider).FirstOrDefaultAsync(ct)
               ?? throw new NotFoundException(nameof(Category), request.Id);
    }
}
