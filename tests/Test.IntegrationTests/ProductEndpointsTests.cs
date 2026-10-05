using Test.Application.Common.Models;
using Test.Application.Features.Auth.DTOs;
using Test.Application.Features.Categories.DTOs;
using Test.Application.Features.Products.DTOs;

namespace Test.IntegrationTests;

[Collection(ApiCollection.Name)]
public class ProductEndpointsTests(ApiFactory factory)
{
    private async Task<CategoryDto> CreateCategoryAsync(HttpClient admin)
    {
        var response = await admin.PostJsonAsync("/api/categories", new { name = $"Cat-{Guid.NewGuid():N}" });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.ReadApiAsync<CategoryDto>()).Data!;
    }

    private static object ProductBody(Guid categoryId, string name, decimal price = 99.90m)
        => new { name, description = "test product", price, stock = 5, categoryId };

    [Fact]
    public async Task GetProducts_Anonymous_ReturnsPagedResult()
    {
        var response = await factory.CreateClient().GetAsync("/api/products?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadApiAsync<PagedResult<ProductDto>>();
        body.Success.Should().BeTrue();
        body.Data!.Page.Should().Be(1);
        body.Data.PageSize.Should().Be(5);
        body.Data.TotalPages.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetProducts_InvalidPageSize_Returns400()
    {
        var response = await factory.CreateClient().GetAsync("/api/products?pageSize=1000");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().PostJsonAsync("/api/products", ProductBody(Guid.NewGuid(), "x"));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProduct_AsNormalUser_Returns403()
    {
        var register = await factory.CreateClient().PostJsonAsync("/api/auth/register",
            new { firstName = "Normal", lastName = "User", email = $"u-{Guid.NewGuid():N}@example.com", password = "Passw0rdX" });
        var token = (await register.ReadApiAsync<AuthResponse>()).Data!.AccessToken;

        var response = await factory.CreateClient().WithBearer(token)
            .PostJsonAsync("/api/products", ProductBody(Guid.NewGuid(), "x"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateProduct_AsAdmin_Returns201_AndAppearsInFilteredList()
    {
        var admin = await factory.CreateAdminClientAsync();
        var category = await CreateCategoryAsync(admin);
        var unique = $"Phone-{Guid.NewGuid():N}";

        var create = await admin.PostJsonAsync("/api/products", ProductBody(category.Id, unique, 1234.50m));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await create.ReadApiAsync<ProductDto>()).Data!;
        created.CategoryName.Should().Be(category.Name);

        var list = await factory.CreateClient().GetAsync($"/api/products?search={unique}&categoryId={category.Id}&minPrice=1000");
        var page = (await list.ReadApiAsync<PagedResult<ProductDto>>()).Data!;
        page.TotalCount.Should().Be(1);
        page.Items.Single().Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task CreateProduct_InvalidPrice_Returns400()
    {
        var admin = await factory.CreateAdminClientAsync();
        var category = await CreateCategoryAsync(admin);

        var response = await admin.PostJsonAsync("/api/products", ProductBody(category.Id, "Bad", price: 0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadApiAsync<object>()).Errors.Select(e => e.Field).Should().Contain("Price");
    }

    [Fact]
    public async Task ProductLifecycle_UpdateStock_Delete_SoftDeletes()
    {
        var admin = await factory.CreateAdminClientAsync();
        var category = await CreateCategoryAsync(admin);
        var product = (await (await admin.PostJsonAsync("/api/products", ProductBody(category.Id, $"P-{Guid.NewGuid():N}")))
            .ReadApiAsync<ProductDto>()).Data!;

        var patch = await admin.PatchAsJsonAsync($"/api/products/{product.Id}/stock", new { stock = 77 }, TestJson.Options);
        patch.StatusCode.Should().Be(HttpStatusCode.OK);
        (await patch.ReadApiAsync<ProductDto>()).Data!.Stock.Should().Be(77);

        (await admin.DeleteAsync($"/api/products/{product.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await factory.CreateClient().GetAsync($"/api/products/{product.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
