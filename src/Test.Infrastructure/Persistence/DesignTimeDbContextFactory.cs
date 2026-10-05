using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Test.Infrastructure.Persistence;

/// <summary>
/// 'dotnet ef' için: uygulamayı (ve seed/migrate kodunu) çalıştırmadan DbContext üretir.
/// Gerçek DB'ye bağlanan komutlar için ConnectionStrings__DefaultConnection env değişkenini ayarlayın.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                               ?? "Host=localhost;Port=5432;Database=test_db;Username=postgres";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
