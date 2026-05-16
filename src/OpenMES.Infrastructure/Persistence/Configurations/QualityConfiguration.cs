using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence.Configurations;

public sealed class QualityCheckConfiguration : IEntityTypeConfiguration<QualityCheck>
{
    public void Configure(EntityTypeBuilder<QualityCheck> b)
    {
        b.ToTable("quality_checks");
        b.HasKey(c => c.Id);
        b.Property(c => c.Title).IsRequired().HasMaxLength(256);
        b.Property(c => c.Unit).HasMaxLength(32);
        b.Property(c => c.Instructions).HasMaxLength(2048);
        b.Property(c => c.MinValue).HasPrecision(18, 6);
        b.Property(c => c.MaxValue).HasPrecision(18, 6);
        b.HasOne(c => c.Operation).WithMany().HasForeignKey(c => c.OperationId);
    }
}

public sealed class QualityResultConfiguration : IEntityTypeConfiguration<QualityResult>
{
    public void Configure(EntityTypeBuilder<QualityResult> b)
    {
        b.ToTable("quality_results");
        b.HasKey(r => r.Id);
        b.Property(r => r.NumericValue).HasPrecision(18, 6);
        b.Property(r => r.TextValue).HasMaxLength(1024);
        b.Property(r => r.Notes).HasMaxLength(1024);
        b.HasOne(r => r.QualityCheck).WithMany().HasForeignKey(r => r.QualityCheckId);
        b.HasOne(r => r.Job).WithMany().HasForeignKey(r => r.JobId);
        b.HasIndex(r => r.JobId);
    }
}
