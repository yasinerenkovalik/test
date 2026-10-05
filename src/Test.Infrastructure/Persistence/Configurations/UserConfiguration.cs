using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Test.Domain.Entities;

namespace Test.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(x => x.PhoneNumber).HasMaxLength(30);
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
