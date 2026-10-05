namespace Test.Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<Unit>;
