using System.Text.Json.Serialization;
using Test.Application.Features.Products.DTOs;

namespace Test.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(IApplicationDbContext db, IMapper mapper)
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await db.Products.Include(p => p.Category)
                          .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
                      ?? throw new NotFoundException(nameof(Product), request.Id);

        if (product.CategoryId != request.CategoryId)
        {
            // Yeni kategori tracked olarak yüklenir; navigation save sonrası güncellenir.
            _ = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct)
                ?? throw new NotFoundException(nameof(Category), request.CategoryId);
        }

        product.Update(request.Name.Trim(), request.Description?.Trim(), request.Price,
            request.Stock, request.CategoryId, request.IsActive);
        await db.SaveChangesAsync(ct);

        return mapper.Map<ProductDto>(product);
    }
}
