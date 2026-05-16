using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(u => u.Id);
        b.Property(u => u.Code).IsRequired().HasMaxLength(32);
        b.HasIndex(u => u.Code).IsUnique();
        b.Property(u => u.DisplayName).IsRequired().HasMaxLength(128);
        b.Property(u => u.Role).IsRequired().HasMaxLength(32);
    }
}
