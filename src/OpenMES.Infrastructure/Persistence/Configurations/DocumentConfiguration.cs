using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> b)
    {
        b.ToTable("documents");
        b.HasKey(d => d.Id);
        b.Property(d => d.Title).IsRequired().HasMaxLength(256);
        b.Property(d => d.UrlOrPath).IsRequired().HasMaxLength(1024);
        b.Property(d => d.PartNumber).HasMaxLength(64);
        b.Property(d => d.Revision).HasMaxLength(16);
        b.Property(d => d.OperationCode).HasMaxLength(32);
        b.Property(d => d.ResourceCode).HasMaxLength(32);
        b.HasMany(d => d.Links).WithOne(l => l.Document!).HasForeignKey(l => l.DocumentId);
        b.HasIndex(d => new { d.PartNumber, d.Revision });
        b.HasIndex(d => d.IsObsolete);
    }
}

public sealed class DocumentLinkConfiguration : IEntityTypeConfiguration<DocumentLink>
{
    public void Configure(EntityTypeBuilder<DocumentLink> b)
    {
        b.ToTable("document_links");
        b.HasKey(l => l.Id);
        b.Property(l => l.ScopeKey).IsRequired().HasMaxLength(128);
        b.HasIndex(l => new { l.Scope, l.ScopeKey });
    }
}
