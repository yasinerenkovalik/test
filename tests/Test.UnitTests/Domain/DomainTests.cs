namespace Test.UnitTests.Domain;

public class DomainTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Product_Create_NonPositivePrice_Throws(decimal price)
    {
        var act = () => Product.Create("Phone", null, price, 1, Guid.NewGuid());
        act.Should().Throw<BusinessException>();
    }

    [Fact]
    public void Product_SetStock_Negative_Throws()
    {
        var product = Product.Create("Phone", null, 10m, 1, Guid.NewGuid());
        var act = () => product.SetStock(-1);
        act.Should().Throw<BusinessException>();
    }

    [Fact]
    public void User_Create_NormalizesEmail()
    {
        var user = User.Create("Ada", "Lovelace", "  ADA@Example.com ", "hash", null);
        user.Email.Should().Be("ada@example.com");
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_IsActive_DependsOnExpiryAndRevocation()
    {
        var now = DateTime.UtcNow;
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", now.AddDays(1));

        token.IsActive(now).Should().BeTrue();
        token.IsActive(now.AddDays(2)).Should().BeFalse();

        token.Revoke(now);
        token.IsActive(now).Should().BeFalse();
    }
}
