using Test.Application.Features.Products.DTOs;

namespace Test.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(IApplicationDbContext db, IMapper mapper)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // Kategori yüklenir (tracked) -> EF navigation'ı otomatik doldurur, DTO'da CategoryName gelir.
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct)
                       ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var product = Product.Create(request.Name.Trim(), request.Description?.Trim(),
            request.Price, request.Stock, category.Id, request.IsActive);

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        return mapper.Map<ProductDto>(product);
    }
}
