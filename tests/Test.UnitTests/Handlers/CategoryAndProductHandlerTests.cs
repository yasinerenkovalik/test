using Test.Application.Common.Exceptions;
using Test.Application.Features.Categories.Commands.CreateCategory;
using Test.Application.Features.Categories.Commands.DeleteCategory;
using Test.Application.Features.Products.Commands.CreateProduct;
using Test.Application.Features.Products.Commands.DeleteProduct;
using Test.Application.Features.Products.Commands.UpdateProductStock;
using Test.Application.Features.Products.Queries.GetProducts;
using Test.Application.Common.Models;
using Test.Infrastructure.Persistence;

namespace Test.UnitTests.Handlers;

public class CategoryAndProductHandlerTests
{
    private readonly FakeCurrentUserService _currentUser = new() { UserId = Guid.NewGuid(), IsAdmin = true };
    private readonly ApplicationDbContext _db;

    public CategoryAndProductHandlerTests() => _db = TestSetup.CreateDb(_currentUser);

    private async Task<Category> SeedCategoryAsync(string name = "Electronics")
    {
        var category = Category.Create(name, null);
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    private async Task<Product> SeedProductAsync(Category category, string name, decimal price, int stock = 1, bool active = true)
    {
        var product = Product.Create(name, $"{name} description", price, stock, category.Id, active);
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    // ---------------- Category ----------------
    [Fact]
    public async Task CreateCategory_DuplicateName_ThrowsBusinessException()
    {
        var handler = new CreateCategoryCommandHandler(_db, TestSetup.CreateMapper());
        await handler.Handle(new CreateCategoryCommand("Books"), CancellationToken.None);

        Func<Task> act = () => handler.Handle(new CreateCategoryCommand("  books "), CancellationToken.None);
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task DeleteCategory_WithProducts_ThrowsBusinessException()
    {
        var category = await SeedCategoryAsync();
        await SeedProductAsync(category, "Phone", 100);

        Func<Task> act = () => new DeleteCategoryCommandHandler(_db)
            .Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task DeleteCategory_Empty_IsSoftDeleted()
    {
        var category = await SeedCategoryAsync();

        await new DeleteCategoryCommandHandler(_db).Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

        (await _db.Categories.AnyAsync()).Should().BeFalse(); // global filter gizler
        var raw = await _db.Categories.IgnoreQueryFilters().SingleAsync(c => c.Id == category.Id);
        raw.IsDeleted.Should().BeTrue();
        raw.DeletedDate.Should().NotBeNull();
    }

    // ---------------- Product ----------------
    [Fact]
    public async Task CreateProduct_UnknownCategory_ThrowsNotFound()
    {
        var handler = new CreateProductCommandHandler(_db, TestSetup.CreateMapper());

        Func<Task> act = () => handler.Handle(
            new CreateProductCommand("Phone", null, 10m, 1, Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateProduct_Valid_ReturnsDtoAndFillsAuditFields()
    {
        var category = await SeedCategoryAsync("Phones");
        var handler = new CreateProductCommandHandler(_db, TestSetup.CreateMapper());

        var dto = await handler.Handle(
            new CreateProductCommand("Phone X", "desc", 999.99m, 5, category.Id), CancellationToken.None);

        dto.CategoryName.Should().Be("Phones");
        dto.Price.Should().Be(999.99m);

        var entity = await _db.Products.SingleAsync();
        entity.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entity.CreatedBy.Should().Be(_currentUser.UserId);
    }

    [Fact]
    public async Task UpdateProductStock_SetsNewStockAndUpdatedDate()
    {
        var category = await SeedCategoryAsync();
        var product = await SeedProductAsync(category, "Phone", 100, stock: 1);

        var dto = await new UpdateProductStockCommandHandler(_db, TestSetup.CreateMapper())
            .Handle(new UpdateProductStockCommand { Id = product.Id, Stock = 42 }, CancellationToken.None);

        dto.Stock.Should().Be(42);
        (await _db.Products.SingleAsync()).UpdatedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteProduct_IsSoftDeletedAndHiddenFromQueries()
    {
        var category = await SeedCategoryAsync();
        var product = await SeedProductAsync(category, "Phone", 100);

        await new DeleteProductCommandHandler(_db).Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        (await _db.Products.AnyAsync()).Should().BeFalse();
        (await _db.Products.IgnoreQueryFilters().SingleAsync()).IsDeleted.Should().BeTrue();
    }

    // ---------------- GetProducts ----------------
    private GetProductsQueryHandler QueryHandler(bool admin = true)
        => new(_db, new FakeCurrentUserService { UserId = Guid.NewGuid(), IsAdmin = admin }, TestSetup.CreateMapper());

    private async Task<(Category Phones, Category Books)> SeedCatalogAsync()
    {
        var phones = await SeedCategoryAsync("Phones");
        var books = await SeedCategoryAsync("Books");
        await SeedProductAsync(phones, "Phone A", 100, stock: 10);
        await SeedProductAsync(phones, "Phone B", 500, stock: 5);
        await SeedProductAsync(phones, "Phone C", 900, stock: 1, active: false);
        await SeedProductAsync(books, "Clean Code", 50, stock: 20);
        await SeedProductAsync(books, "Refactoring", 60, stock: 8);
        return (phones, books);
    }

    [Fact]
    public async Task GetProducts_SearchFilter_MatchesNameCaseInsensitive()
    {
        await SeedCatalogAsync();

        var result = await QueryHandler().Handle(new GetProductsQuery { Search = "PHONE" }, CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Items.Should().OnlyContain(p => p.Name.StartsWith("Phone"));
    }

    [Fact]
    public async Task GetProducts_CategoryAndPriceFilters_AreCombined()
    {
        var (phones, _) = await SeedCatalogAsync();

        var result = await QueryHandler().Handle(
            new GetProductsQuery { CategoryId = phones.Id, MinPrice = 200, MaxPrice = 600 }, CancellationToken.None);

        result.Items.Should().ContainSingle().Which.Name.Should().Be("Phone B");
    }

    [Fact]
    public async Task GetProducts_Paging_ReturnsExpectedSliceAndTotals()
    {
        await SeedCatalogAsync(); // admin 5 ürün görür

        var result = await QueryHandler().Handle(
            new GetProductsQuery { Page = 3, PageSize = 2, SortBy = ProductSortBy.Name, SortDirection = SortDirection.Asc },
            CancellationToken.None);

        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProducts_SortByPriceDescending_ReturnsMostExpensiveFirst()
    {
        await SeedCatalogAsync();

        var result = await QueryHandler().Handle(
            new GetProductsQuery { SortBy = ProductSortBy.Price, SortDirection = SortDirection.Desc }, CancellationToken.None);

        result.Items.Select(p => p.Price).Should().BeInDescendingOrder();
        result.Items.First().Name.Should().Be("Phone C");
    }

    [Fact]
    public async Task GetProducts_NonAdmin_SeesOnlyActiveProducts()
    {
        await SeedCatalogAsync();

        var asUser = await QueryHandler(admin: false).Handle(new GetProductsQuery(), CancellationToken.None);
        var asAdmin = await QueryHandler(admin: true).Handle(new GetProductsQuery(), CancellationToken.None);

        asUser.TotalCount.Should().Be(4);
        asUser.Items.Should().OnlyContain(p => p.IsActive);
        asAdmin.TotalCount.Should().Be(5);
    }
}
