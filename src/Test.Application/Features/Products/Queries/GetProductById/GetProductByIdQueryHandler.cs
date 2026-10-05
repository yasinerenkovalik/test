using Test.Application.Features.Products.DTOs;

namespace Test.Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(
    IApplicationDbContext db, ICurrentUserService currentUser, IMapper mapper)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        IQueryable<Product> query = db.Products.AsNoTracking().Where(p => p.Id == request.Id);

        // Pasif ürünü sadece Admin görebilir; diğerleri için "yok" gibi davranılır.
        if (!currentUser.IsAdmin) query = query.Where(p => p.IsActive);

        return await query.ProjectTo<ProductDto>(mapper.ConfigurationProvider).FirstOrDefaultAsync(ct)
               ?? throw new NotFoundException(nameof(Product), request.Id);
    }
}
