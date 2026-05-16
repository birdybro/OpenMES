using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence.Configurations;

public sealed class ResourceScheduleEntryConfiguration : IEntityTypeConfiguration<ResourceScheduleEntry>
{
    public void Configure(EntityTypeBuilder<ResourceScheduleEntry> b)
    {
        b.ToTable("resource_schedule");
        b.HasKey(s => s.Id);
        b.Property(s => s.PlannedQuantity).HasPrecision(18, 4);
        b.Property(s => s.Notes).HasMaxLength(512);
        b.HasOne(s => s.Resource).WithMany().HasForeignKey(s => s.ResourceId);
        b.HasOne(s => s.Job).WithMany().HasForeignKey(s => s.JobId);
        b.HasIndex(s => new { s.ResourceId, s.PlannedStartUtc });
    }
}
