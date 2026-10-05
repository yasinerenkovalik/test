namespace Test.Application.Features.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteCategoryCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
                       ?? throw new NotFoundException(nameof(Category), request.Id);

        // İş kuralı: ürünü olan kategori silinemez (global filter silinmiş ürünleri zaten saymaz).
        if (await db.Products.AnyAsync(p => p.CategoryId == request.Id, ct))
            throw new BusinessException("Category cannot be deleted because it contains products.");

        // Remove -> interceptor bunu soft delete'e çevirir (IsDeleted + DeletedDate).
        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
