using System.Text.Json.Serialization;
using Test.Application.Features.Categories.DTOs;

namespace Test.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(IApplicationDbContext db, IMapper mapper)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, ct)
                       ?? throw new NotFoundException(nameof(Category), request.Id);

        var name = request.Name.Trim();
        var lower = name.ToLower();
        if (await db.Categories.AnyAsync(c => c.Id != request.Id && c.Name.ToLower() == lower, ct))
            throw new BusinessException($"A category named '{name}' already exists.");

        category.Update(name, request.Description?.Trim(), request.IsActive);
        await db.SaveChangesAsync(ct);

        return mapper.Map<CategoryDto>(category);
    }
}
