using Test.Application.Features.Products.DTOs;

namespace Test.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid CategoryId,
    bool IsActive = true) : IRequest<ProductDto>;
