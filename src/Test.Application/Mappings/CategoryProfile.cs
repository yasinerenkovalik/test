using Test.Application.Features.Categories.DTOs;

namespace Test.Application.Mappings;

public sealed class CategoryProfile : Profile
{
    public CategoryProfile() => CreateMap<Category, CategoryDto>();
}
