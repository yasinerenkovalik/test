namespace Test.Application.Common.Interfaces;

/// <summary>
/// Generic repository yerine DbContext soyutlaması: handler'lar IQueryable'ın
/// gücünden (Where/ProjectTo/Skip/Take) tam faydalanır, filtreler DB tarafında çalışır.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
