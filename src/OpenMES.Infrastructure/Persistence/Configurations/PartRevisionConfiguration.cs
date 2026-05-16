using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence.Configurations;

public sealed class PartRevisionConfiguration : IEntityTypeConfiguration<PartRevision>
{
    public void Configure(EntityTypeBuilder<PartRevision> b)
    {
        b.ToTable("part_revisions");
        b.HasKey(r => r.Id);
        b.Property(r => r.Revision).IsRequired().HasMaxLength(16);
        b.Property(r => r.Notes).HasMaxLength(1024);
        b.HasIndex(r => new { r.PartId, r.Revision }).IsUnique();
        b.HasMany(r => r.Operations).WithOne(o => o.PartRevision!).HasForeignKey(o => o.PartRevisionId);
    }
}
