using Test.Application.Features.Categories.DTOs;

namespace Test.Application.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler(
    IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        IQueryable<Category> query = db.Categories.AsNoTracking();

        // Admin olmayanlar sadece aktif kategorileri görür.
        if (!currentUser.IsAdmin) query = query.Where(c => c.IsActive);

        return await query.OrderBy(c => c.Name)
            .ProjectTo<CategoryDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
