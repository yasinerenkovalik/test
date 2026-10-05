using Test.Application.Common.Models;
using Test.Application.Features.Products.DTOs;

namespace Test.Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQuery : IRequest<PagedResult<ProductDto>>
{
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? IsActive { get; init; }
    public ProductSortBy SortBy { get; init; } = ProductSortBy.CreatedDate;
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
