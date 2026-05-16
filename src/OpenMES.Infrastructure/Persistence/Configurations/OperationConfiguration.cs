using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence.Configurations;

public sealed class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> b)
    {
        b.ToTable("operations");
        b.HasKey(o => o.Id);
        b.Property(o => o.OperationCode).IsRequired().HasMaxLength(32);
        b.Property(o => o.Description).HasMaxLength(512);
        b.Property(o => o.PreferredResourceCode).HasMaxLength(32);
        b.Property(o => o.StandardRunTimeMinutes).HasPrecision(10, 4);
        b.Property(o => o.StandardSetupTimeMinutes).HasPrecision(10, 4);
        b.HasIndex(o => new { o.PartRevisionId, o.OperationCode }).IsUnique();
    }
}
