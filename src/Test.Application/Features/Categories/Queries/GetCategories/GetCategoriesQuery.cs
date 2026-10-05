using Test.Application.Features.Categories.DTOs;

namespace Test.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
