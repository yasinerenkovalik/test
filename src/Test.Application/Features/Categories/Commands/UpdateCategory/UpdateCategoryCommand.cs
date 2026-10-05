using System.Text.Json.Serialization;
using Test.Application.Features.Categories.DTOs;

namespace Test.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand : IRequest<CategoryDto>
{
    /// <summary>Route'tan gelir; body'den okunmaz.</summary>
    [JsonIgnore] public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}
