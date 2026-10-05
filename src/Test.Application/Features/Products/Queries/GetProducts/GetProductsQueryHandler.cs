using Test.Application.Common.Models;
using Test.Application.Features.Products.DTOs;

namespace Test.Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler(
    IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        // Tüm filtre/sıralama/sayfalama IQueryable üzerinde: SQL'e çevrilir, memory'ye veri çekilmez.
        IQueryable<Product> query = db.Products.AsNoTracking();

        if (!currentUser.IsAdmin)
            query = query.Where(p => p.IsActive);
        else if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term)
                                     || (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        var asc = request.SortDirection == SortDirection.Asc;
        IOrderedQueryable<Product> ordered = request.SortBy switch
        {
            ProductSortBy.Name => asc ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            ProductSortBy.Price => asc ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
            ProductSortBy.Stock => asc ? query.OrderBy(p => p.Stock) : query.OrderByDescending(p => p.Stock),
            _ => asc ? query.OrderBy(p => p.CreatedDate) : query.OrderByDescending(p => p.CreatedDate)
        };
        // İkincil sıralama: sayfalar arasında kararlı sonuç.
        ordered = ordered.ThenBy(p => p.Id);

        var totalCount = await query.CountAsync(ct);
        var items = await ordered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<ProductDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(items, request.Page, request.PageSize, totalCount);
    }
}
