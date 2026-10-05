using System.Text.Json.Serialization;
using Test.Application.Features.Products.DTOs;

namespace Test.Application.Features.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand : IRequest<ProductDto>
{
    /// <summary>Route'tan gelir; body'den okunmaz.</summary>
    [JsonIgnore] public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public Guid CategoryId { get; init; }
    public bool IsActive { get; init; } = true;
}
