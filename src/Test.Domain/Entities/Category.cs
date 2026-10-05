using Test.Domain.Common;

namespace Test.Domain.Entities;

public sealed class Category : BaseEntity
{
    private Category() { } // EF Core

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<Product> Products { get; private set; } = new List<Product>();

    public static Category Create(string name, string? description)
        => new() { Name = name, Description = description, IsActive = true };

    public void Update(string name, string? description, bool isActive)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
    }
}
