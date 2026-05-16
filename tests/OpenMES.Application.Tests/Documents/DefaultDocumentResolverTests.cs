using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Tests.Documents;

public class DefaultDocumentResolverTests
{
    [Fact]
    public async Task Returns_documents_matching_part_and_revision()
    {
        using var h = new TestHarness();
        h.Db.Documents.AddRange(
            new Document { Title = "WI", DocumentType = DocumentType.WorkInstruction, PartNumber = "P1", Revision = "A", IsReleased = true, UrlOrPath = "p1.pdf" },
            new Document { Title = "Other part", DocumentType = DocumentType.Drawing, PartNumber = "P2", Revision = "A", IsReleased = true, UrlOrPath = "p2.pdf" });
        await h.Db.SaveChangesAsync();

        var docs = await h.Resolver.ResolveAsync(new DocumentResolutionContext(1, "P1", "A"));
        Assert.Single(docs);
        Assert.Equal("WI", docs[0].Title);
    }

    [Fact]
    public async Task Excludes_obsolete_and_unreleased()
    {
        using var h = new TestHarness();
        h.Db.Documents.AddRange(
            new Document { Title = "Released", DocumentType = DocumentType.Drawing, PartNumber = "P1", Revision = "A", IsReleased = true, UrlOrPath = "rel.pdf" },
            new Document { Title = "Obsolete", DocumentType = DocumentType.Drawing, PartNumber = "P1", Revision = "A", IsReleased = true, IsObsolete = true, UrlOrPath = "obs.pdf" },
            new Document { Title = "Unreleased", DocumentType = DocumentType.Drawing, PartNumber = "P1", Revision = "A", IsReleased = false, UrlOrPath = "draft.pdf" });
        await h.Db.SaveChangesAsync();

        var docs = await h.Resolver.ResolveAsync(new DocumentResolutionContext(1, "P1", "A"));
        var titles = docs.Select(d => d.Title).ToHashSet();
        Assert.Contains("Released", titles);
        Assert.DoesNotContain("Obsolete", titles);
        Assert.DoesNotContain("Unreleased", titles);
    }

    [Fact]
    public async Task Operation_specific_documents_rank_above_part_level()
    {
        using var h = new TestHarness();
        h.Db.Documents.AddRange(
            new Document { Title = "Part WI", DocumentType = DocumentType.WorkInstruction, PartNumber = "P1", Revision = "A", IsReleased = true, UrlOrPath = "p.pdf" },
            new Document { Title = "Op-specific WI", DocumentType = DocumentType.WorkInstruction, PartNumber = "P1", Revision = "A", OperationCode = "OP020", IsReleased = true, UrlOrPath = "op.pdf" });
        await h.Db.SaveChangesAsync();

        var docs = await h.Resolver.ResolveAsync(new DocumentResolutionContext(1, "P1", "A", OperationCode: "OP020"));
        Assert.Equal("Op-specific WI", docs[0].Title);
        Assert.Equal("Part WI", docs[1].Title);
    }

    [Fact]
    public async Task Resource_scoped_documents_match_via_resource_code()
    {
        using var h = new TestHarness();
        h.Db.Documents.Add(new Document
        {
            Title = "CNC-01 safety",
            DocumentType = DocumentType.Safety,
            ResourceCode = "CNC-01",
            IsReleased = true,
            UrlOrPath = "safety.pdf"
        });
        await h.Db.SaveChangesAsync();

        var docs = await h.Resolver.ResolveAsync(new DocumentResolutionContext(1, "ANY", "A", ResourceCode: "CNC-01"));
        Assert.Single(docs);
        Assert.Equal("CNC-01 safety", docs[0].Title);
    }

    [Fact]
    public async Task Explicit_document_link_pulls_in_a_matching_document()
    {
        using var h = new TestHarness();
        var doc = new Document
        {
            Title = "Linked doc",
            DocumentType = DocumentType.Procedure,
            IsReleased = true,
            UrlOrPath = "linked.pdf"
        };
        h.Db.Documents.Add(doc);
        await h.Db.SaveChangesAsync();
        h.Db.DocumentLinks.Add(new DocumentLink
        {
            DocumentId = doc.Id,
            Scope = DocumentScope.Revision,
            ScopeKey = "P1/A"
        });
        await h.Db.SaveChangesAsync();

        var docs = await h.Resolver.ResolveAsync(new DocumentResolutionContext(1, "P1", "A"));
        Assert.Contains(docs, d => d.Title == "Linked doc");
    }
}
