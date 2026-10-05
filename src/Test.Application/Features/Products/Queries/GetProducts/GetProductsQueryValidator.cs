using Test.Application.Common.Models;
using Test.Application.Features.Products.DTOs;

namespace Test.Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0).When(x => x.MinPrice.HasValue);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0).When(x => x.MaxPrice.HasValue);
        RuleFor(x => x).Must(x => x.MinPrice <= x.MaxPrice)
            .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithName("MinPrice").WithMessage("MinPrice cannot be greater than MaxPrice.");
        RuleFor(x => x.Search).MaximumLength(100);
    }
}
