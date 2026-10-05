using Test.Domain.Common;
using Test.Domain.Exceptions;

namespace Test.Domain.Entities;

public sealed class Product : BaseEntity
{
    private Product() { } // EF Core

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public static Product Create(string name, string? description, decimal price, int stock,
        Guid categoryId, bool isActive = true)
    {
        EnsureValidPrice(price);
        EnsureValidStock(stock);

        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            Stock = stock,
            CategoryId = categoryId,
            IsActive = isActive
        };
    }

    public void Update(string name, string? description, decimal price, int stock,
        Guid categoryId, bool isActive)
    {
        EnsureValidPrice(price);
        EnsureValidStock(stock);

        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        CategoryId = categoryId;
        IsActive = isActive;
    }

    public void SetStock(int stock)
    {
        EnsureValidStock(stock);
        Stock = stock;
    }

    // İş kuralları domain içinde: validator atlansa bile geçersiz entity oluşamaz.
    private static void EnsureValidPrice(decimal price)
    {
        if (price <= 0) throw new BusinessException("Price must be greater than zero.");
    }

    private static void EnsureValidStock(int stock)
    {
        if (stock < 0) throw new BusinessException("Stock cannot be negative.");
    }
}
