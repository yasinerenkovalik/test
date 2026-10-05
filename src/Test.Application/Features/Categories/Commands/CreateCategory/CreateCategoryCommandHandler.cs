using Test.Application.Features.Categories.DTOs;

namespace Test.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(IApplicationDbContext db, IMapper mapper)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var name = request.Name.Trim();
        var lower = name.ToLower();

        if (await db.Categories.AnyAsync(c => c.Name.ToLower() == lower, ct))
            throw new BusinessException($"A category named '{name}' already exists.");

        var category = Category.Create(name, request.Description?.Trim());
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return mapper.Map<CategoryDto>(category);
    }
}
