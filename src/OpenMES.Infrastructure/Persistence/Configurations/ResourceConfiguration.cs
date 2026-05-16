using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence.Configurations;

public sealed class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> b)
    {
        b.ToTable("resources");
        b.HasKey(r => r.Id);
        b.Property(r => r.Code).IsRequired().HasMaxLength(32);
        b.HasIndex(r => r.Code).IsUnique();
        b.Property(r => r.Name).IsRequired().HasMaxLength(128);
        b.Property(r => r.Location).HasMaxLength(128);
    }
}
