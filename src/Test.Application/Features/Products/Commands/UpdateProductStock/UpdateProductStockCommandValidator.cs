using System.Text.Json.Serialization;
using Test.Application.Features.Products.DTOs;

namespace Test.Application.Features.Products.Commands.UpdateProductStock;

public sealed class UpdateProductStockCommandValidator : AbstractValidator<UpdateProductStockCommand>
{
    public UpdateProductStockCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}
