using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence.Configurations;

public sealed class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> b)
    {
        b.ToTable("parts");
        b.HasKey(p => p.Id);
        b.Property(p => p.PartNumber).IsRequired().HasMaxLength(64);
        b.HasIndex(p => p.PartNumber).IsUnique();
        b.Property(p => p.Description).HasMaxLength(512);
        b.Property(p => p.UnitOfMeasure).HasMaxLength(16);
        b.HasMany(p => p.Revisions).WithOne(r => r.Part!).HasForeignKey(r => r.PartId);
    }
}
