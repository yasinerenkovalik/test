using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Test.Application.Common.Models;
using Test.Application.Common.Security;
using Test.Application.Features.Categories.Commands.CreateCategory;
using Test.Application.Features.Categories.Commands.DeleteCategory;
using Test.Application.Features.Categories.Commands.UpdateCategory;
using Test.Application.Features.Categories.DTOs;
using Test.Application.Features.Categories.Queries.GetCategories;
using Test.Application.Features.Categories.Queries.GetCategoryById;

namespace Test.API.Controllers;

[Route("api/categories")]
public sealed class CategoriesController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Kategorileri listeler (herkese açık; Admin olmayanlar sadece aktifleri görür).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Success(await Sender.Send(new GetCategoriesQuery(), ct));

    /// <summary>Kategori detayını getirir (herkese açık).</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Success(await Sender.Send(new GetCategoryByIdQuery(id), ct));

    /// <summary>Yeni kategori oluşturur (Admin).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateCategoryCommand command, CancellationToken ct)
        => Success(await Sender.Send(command, ct), "Category created successfully.", StatusCodes.Status201Created);

    /// <summary>Kategoriyi günceller (Admin).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryCommand command, CancellationToken ct)
        => Success(await Sender.Send(command with { Id = id }, ct), "Category updated successfully.");

    /// <summary>Kategoriyi soft delete ile siler (Admin). Ürünü olan kategori silinemez.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await Sender.Send(new DeleteCategoryCommand(id), ct);
        return Success<object?>(null, "Category deleted successfully.");
    }
}
