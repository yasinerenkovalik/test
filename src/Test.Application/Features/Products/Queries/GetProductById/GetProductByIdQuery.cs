using Test.Application.Features.Products.DTOs;

namespace Test.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;
