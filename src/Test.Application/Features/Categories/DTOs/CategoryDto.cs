namespace Test.Application.Features.Categories.DTOs;

public sealed record CategoryDto(Guid Id, string Name, string? Description, bool IsActive, DateTime CreatedDate);
