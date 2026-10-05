using Test.Application.Features.Categories.DTOs;

namespace Test.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string? Description = null) : IRequest<CategoryDto>;
