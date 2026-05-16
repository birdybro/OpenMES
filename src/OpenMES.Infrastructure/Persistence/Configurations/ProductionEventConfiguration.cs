using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence.Configurations;

public sealed class ProductionEventConfiguration : IEntityTypeConfiguration<ProductionEvent>
{
    public void Configure(EntityTypeBuilder<ProductionEvent> b)
    {
        b.ToTable("production_events");
        b.HasKey(e => e.Id);
        b.Property(e => e.Quantity).HasPrecision(18, 4);
        b.Property(e => e.ReasonCode).HasMaxLength(64);
        b.Property(e => e.Notes).HasMaxLength(1024);
        b.Property(e => e.RawScanValue).HasMaxLength(512);

        b.HasOne(e => e.Job).WithMany().HasForeignKey(e => e.JobId).IsRequired(false);
        b.HasOne(e => e.Resource).WithMany().HasForeignKey(e => e.ResourceId).IsRequired(false);
        b.HasOne(e => e.Operation).WithMany().HasForeignKey(e => e.OperationId).IsRequired(false);
        b.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).IsRequired(false);

        b.HasIndex(e => new { e.JobId, e.TimestampUtc });
        b.HasIndex(e => new { e.EventType, e.TimestampUtc });
    }
}

public sealed class ScanEventConfiguration : IEntityTypeConfiguration<ScanEvent>
{
    public void Configure(EntityTypeBuilder<ScanEvent> b)
    {
        b.ToTable("scan_events");
        b.HasKey(e => e.Id);
        b.Property(e => e.RawValue).IsRequired().HasMaxLength(512);
        b.Property(e => e.ParsedType).HasMaxLength(32);
        b.Property(e => e.ParsedKey).HasMaxLength(128);
        b.Property(e => e.ParsedQuantity).HasPrecision(18, 4);
        b.HasIndex(e => e.TimestampUtc);
    }
}
