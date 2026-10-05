using Test.Application.Features.Auth.Commands.Login;
using Test.Application.Features.Auth.Commands.Register;
using Test.Application.Features.Categories.Commands.CreateCategory;
using Test.Application.Features.Products.Commands.CreateProduct;
using Test.Application.Features.Products.Commands.UpdateProduct;
using Test.Application.Features.Products.Commands.UpdateProductStock;
using Test.Application.Features.Products.Queries.GetProducts;
using Test.Application.Features.Users.Commands.ChangePassword;

namespace Test.UnitTests.Validators;

public class ValidatorTests
{
    // ---------- Register ----------
    [Fact]
    public void Register_ValidCommand_IsValid()
    {
        var result = new RegisterCommandValidator()
            .Validate(new RegisterCommand("Ada", "Lovelace", "ada@example.com", "Passw0rdX", "+905551112233"));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Register_InvalidEmail_Fails(string email)
    {
        var result = new RegisterCommandValidator()
            .Validate(new RegisterCommand("Ada", "Lovelace", email, "Passw0rdX"));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Email));
    }

    [Theory]
    [InlineData("short1A")]      // 8 karakterden kısa
    [InlineData("alllowercase1")] // büyük harf yok
    [InlineData("ALLUPPERCASE1")] // küçük harf yok
    [InlineData("NoDigitsHere")]  // rakam yok
    public void Register_WeakPassword_Fails(string password)
    {
        var result = new RegisterCommandValidator()
            .Validate(new RegisterCommand("Ada", "Lovelace", "ada@example.com", password));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Password));
    }

    // ---------- Login ----------
    [Fact]
    public void Login_MissingFields_Fails()
    {
        var result = new LoginCommandValidator().Validate(new LoginCommand("", ""));
        result.Errors.Select(e => e.PropertyName).Should().Contain([nameof(LoginCommand.Email), nameof(LoginCommand.Password)]);
    }

    // ---------- ChangePassword ----------
    [Fact]
    public void ChangePassword_SameAsCurrent_Fails()
    {
        var result = new ChangePasswordCommandValidator()
            .Validate(new ChangePasswordCommand("Passw0rdX", "Passw0rdX"));
        result.IsValid.Should().BeFalse();
    }

    // ---------- Category ----------
    [Fact]
    public void CreateCategory_EmptyName_Fails()
    {
        var result = new CreateCategoryCommandValidator().Validate(new CreateCategoryCommand(""));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    // ---------- Product ----------
    [Theory]
    [InlineData("", 10, 1)]   // isim boş
    [InlineData("Phone", 0, 1)]  // fiyat 0
    [InlineData("Phone", -5, 1)] // fiyat negatif
    [InlineData("Phone", 10, -1)] // stok negatif
    public void CreateProduct_InvalidValues_Fails(string name, decimal price, int stock)
    {
        var result = new CreateProductCommandValidator()
            .Validate(new CreateProductCommand(name, null, price, stock, Guid.NewGuid()));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateProduct_Valid_IsValid()
    {
        var result = new CreateProductCommandValidator()
            .Validate(new CreateProductCommand("Phone", null, 10m, 0, Guid.NewGuid()));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateProduct_InvalidPrice_Fails()
    {
        var command = new UpdateProductCommand { Id = Guid.NewGuid(), Name = "Phone", Price = 0, CategoryId = Guid.NewGuid() };
        new UpdateProductCommandValidator().Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateProductStock_Negative_Fails()
    {
        var command = new UpdateProductStockCommand { Id = Guid.NewGuid(), Stock = -1 };
        new UpdateProductStockCommandValidator().Validate(command).IsValid.Should().BeFalse();
    }

    // ---------- GetProducts ----------
    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void GetProducts_InvalidPaging_Fails(int page, int pageSize)
    {
        var result = new GetProductsQueryValidator().Validate(new GetProductsQuery { Page = page, PageSize = pageSize });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetProducts_MinPriceGreaterThanMax_Fails()
    {
        var result = new GetProductsQueryValidator().Validate(new GetProductsQuery { MinPrice = 100, MaxPrice = 10 });
        result.IsValid.Should().BeFalse();
    }
}
