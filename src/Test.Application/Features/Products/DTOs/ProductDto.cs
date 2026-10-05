namespace Test.Application.Features.Products.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid CategoryId,
    string CategoryName,
    bool IsActive,
    DateTime CreatedDate);
