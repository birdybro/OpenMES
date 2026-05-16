using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence.Configurations;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> b)
    {
        b.ToTable("jobs");
        b.HasKey(j => j.Id);
        b.Property(j => j.JobNumber).IsRequired().HasMaxLength(32);
        b.HasIndex(j => j.JobNumber).IsUnique();
        b.Property(j => j.QuantityOrdered).HasPrecision(18, 4);
        b.Property(j => j.QuantityGood).HasPrecision(18, 4);
        b.Property(j => j.QuantityScrap).HasPrecision(18, 4);
        b.Property(j => j.Notes).HasMaxLength(2048);

        b.HasOne(j => j.PartRevision).WithMany().HasForeignKey(j => j.PartRevisionId);
        b.HasOne(j => j.Resource).WithMany().HasForeignKey(j => j.ResourceId).IsRequired(false);

        b.HasIndex(j => j.Status);
        b.HasIndex(j => j.DueUtc);
    }
}
