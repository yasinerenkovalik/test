namespace Test.Application.Features.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}
