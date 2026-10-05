using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Test.Application.Common.Models;
using Test.Application.Common.Security;
using Test.Application.Features.Products.Commands.CreateProduct;
using Test.Application.Features.Products.Commands.DeleteProduct;
using Test.Application.Features.Products.Commands.UpdateProduct;
using Test.Application.Features.Products.Commands.UpdateProductStock;
using Test.Application.Features.Products.DTOs;
using Test.Application.Features.Products.Queries.GetProductById;
using Test.Application.Features.Products.Queries.GetProducts;

namespace Test.API.Controllers;

[Route("api/products")]
public sealed class ProductsController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>
    /// Ürünleri filtreler, sıralar ve sayfalar (herkese açık katalog; Admin olmayanlar sadece aktif ürünleri görür).
    /// Örn: /api/products?search=phone&amp;minPrice=100&amp;sortBy=Price&amp;sortDirection=Asc&amp;page=1&amp;pageSize=20
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] GetProductsQuery query, CancellationToken ct)
        => Success(await Sender.Send(query, ct));

    /// <summary>Ürün detayını getirir (herkese açık).</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Success(await Sender.Send(new GetProductByIdQuery(id), ct));

    /// <summary>Yeni ürün oluşturur (Admin).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken ct)
        => Success(await Sender.Send(command, ct), "Product created successfully.", StatusCodes.Status201Created);

    /// <summary>Ürünü günceller (Admin).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateProductCommand command, CancellationToken ct)
        => Success(await Sender.Send(command with { Id = id }, ct), "Product updated successfully.");

    /// <summary>Ürünü soft delete ile siler (Admin).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Sender.Send(new DeleteProductCommand(id), ct);
        return Success<object?>(null, "Product deleted successfully.");
    }

    /// <summary>Ürün stoğunu yeni mutlak değere ayarlar (Admin).</summary>
    [HttpPatch("{id:guid}/stock")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStock(Guid id, UpdateProductStockCommand command, CancellationToken ct)
        => Success(await Sender.Send(command with { Id = id }, ct), "Product stock updated successfully.");
}
