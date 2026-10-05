using Test.Application.Features.Categories.DTOs;

namespace Test.Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;
