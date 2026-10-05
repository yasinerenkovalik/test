using System.Text.Json.Serialization;
using Test.Application.Features.Products.DTOs;

namespace Test.Application.Features.Products.Commands.UpdateProductStock;

public sealed class UpdateProductStockCommandHandler(IApplicationDbContext db, IMapper mapper)
    : IRequestHandler<UpdateProductStockCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductStockCommand request, CancellationToken ct)
    {
        var product = await db.Products.Include(p => p.Category)
                          .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
                      ?? throw new NotFoundException(nameof(Product), request.Id);

        product.SetStock(request.Stock);
        await db.SaveChangesAsync(ct);

        return mapper.Map<ProductDto>(product);
    }
}
