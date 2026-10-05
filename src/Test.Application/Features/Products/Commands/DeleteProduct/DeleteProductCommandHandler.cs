namespace Test.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteProductCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct)
                      ?? throw new NotFoundException(nameof(Product), request.Id);

        db.Products.Remove(product); // interceptor soft delete'e çevirir
        await db.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
