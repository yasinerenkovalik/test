using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Test.Domain.Entities;

namespace Test.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(1000);

        // Soft delete edilen kategorinin adı yeniden kullanılabilsin diye kısmi unique index.
        builder.HasIndex(x => x.Name).IsUnique().HasFilter("\"IsDeleted\" = false");
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
