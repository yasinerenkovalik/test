using Test.Application.Features.Products.DTOs;

namespace Test.Application.Mappings;

public sealed class ProductProfile : Profile
{
    // CategoryName, Category.Name'den otomatik flatten edilir.
    public ProductProfile() => CreateMap<Product, ProductDto>();
}
