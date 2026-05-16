using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence.Configurations;

public sealed class MaterialLotConfiguration : IEntityTypeConfiguration<MaterialLot>
{
    public void Configure(EntityTypeBuilder<MaterialLot> b)
    {
        b.ToTable("material_lots");
        b.HasKey(l => l.Id);
        b.Property(l => l.LotCode).IsRequired().HasMaxLength(64);
        b.HasIndex(l => l.LotCode).IsUnique();
        b.Property(l => l.PartNumber).IsRequired().HasMaxLength(64);
        b.HasIndex(l => l.PartNumber);
        b.Property(l => l.UnitOfMeasure).HasMaxLength(16);
        b.Property(l => l.Supplier).HasMaxLength(128);
        b.Property(l => l.Notes).HasMaxLength(1024);
        b.Property(l => l.QuantityOnHand).HasPrecision(18, 4);
    }
}

public sealed class JobMaterialIssueConfiguration : IEntityTypeConfiguration<JobMaterialIssue>
{
    public void Configure(EntityTypeBuilder<JobMaterialIssue> b)
    {
        b.ToTable("job_material_issues");
        b.HasKey(i => i.Id);
        b.Property(i => i.Quantity).HasPrecision(18, 4);
        b.Property(i => i.Notes).HasMaxLength(1024);
        b.HasOne(i => i.Job).WithMany().HasForeignKey(i => i.JobId);
        b.HasOne(i => i.MaterialLot).WithMany().HasForeignKey(i => i.MaterialLotId);
        b.HasIndex(i => i.JobId);
    }
}
