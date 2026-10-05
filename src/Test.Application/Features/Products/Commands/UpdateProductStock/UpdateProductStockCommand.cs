using System.Text.Json.Serialization;
using Test.Application.Features.Products.DTOs;

namespace Test.Application.Features.Products.Commands.UpdateProductStock;

public sealed record UpdateProductStockCommand : IRequest<ProductDto>
{
    /// <summary>Route'tan gelir; body'den okunmaz.</summary>
    [JsonIgnore] public Guid Id { get; init; }
    /// <summary>Yeni mutlak stok değeri.</summary>
    public int Stock { get; init; }
}
