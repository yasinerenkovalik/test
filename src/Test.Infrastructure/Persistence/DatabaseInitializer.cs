using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Test.Application.Common.Interfaces;
using Test.Domain.Entities;
using Test.Domain.Enums;

namespace Test.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, bool seed, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<ApplicationDbContext>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");

        if (db.Database.GetMigrations().Any())
        {
            await db.Database.MigrateAsync(ct);
        }
        else
        {
            // Henüz migration üretilmemişse uygulama yine de açılsın diye (README'ye bakın).
            logger.LogWarning("No EF Core migrations found; falling back to EnsureCreated. " +
                              "Run 'dotnet ef migrations add InitialCreate' for a proper migration history.");
            await db.Database.EnsureCreatedAsync(ct);
        }

        if (seed)
            await SeedAsync(sp, db, logger, ct);
    }

    private static async Task SeedAsync(IServiceProvider sp, ApplicationDbContext db, ILogger logger, CancellationToken ct)
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var hasher = sp.GetRequiredService<IPasswordHasher>();

        if (!await db.Users.AnyAsync(ct))
        {
            // Kimlik bilgileri kodda/README'de değil, development-only config'den (Seed:*) okunur.
            AddSeedUser(db, hasher, config, "Seed:Admin", "Admin", "User", UserRole.Admin, logger);
            AddSeedUser(db, hasher, config, "Seed:User", "Demo", "User", UserRole.User, logger);
        }

        if (!await db.Categories.AnyAsync(ct))
        {
            var electronics = Category.Create("Electronics", "Phones, laptops and accessories");
            var books = Category.Create("Books", "Fiction and non-fiction");
            var home = Category.Create("Home & Kitchen", "Everything for your home");
            db.Categories.AddRange(electronics, books, home);

            db.Products.AddRange(
                Product.Create("Phone X", "6.1 inch smartphone", 15999.90m, 25, electronics.Id),
                Product.Create("Laptop Pro 14", "14 inch laptop, 16 GB RAM", 42999.00m, 10, electronics.Id),
                Product.Create("Wireless Headphones", "Noise cancelling", 3499.50m, 60, electronics.Id),
                Product.Create("Clean Code", "Robert C. Martin", 450.00m, 40, books.Id),
                Product.Create("The Pragmatic Programmer", "Hunt & Thomas", 520.00m, 35, books.Id),
                Product.Create("Coffee Maker", "Programmable, 1.2 L", 1899.00m, 15, home.Id));
        }

        await db.SaveChangesAsync(ct);
    }

    private static void AddSeedUser(ApplicationDbContext db, IPasswordHasher hasher, IConfiguration config,
        string section, string firstName, string lastName, UserRole role, ILogger logger)
    {
        var email = config[$"{section}:Email"];
        var password = config[$"{section}:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("{Section} seed credentials are not configured; skipping.", section);
            return;
        }

        db.Users.Add(User.Create(firstName, lastName, email, hasher.Hash(password), null, role));
    }
}
