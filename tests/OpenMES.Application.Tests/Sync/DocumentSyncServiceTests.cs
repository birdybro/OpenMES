using Microsoft.Extensions.Logging.Abstractions;
using OpenMES.Application.Sync;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Tests.Sync;

public class DocumentSyncServiceTests
{
    private static DocumentSyncService NewService(TestHarness h, params IExternalDocumentConnector[] connectors)
        => new(h.Data, h.Clock, connectors, NullLogger<DocumentSyncService>.Instance);

    [Fact]
    public async Task Empty_connector_list_returns_empty_report()
    {
        using var h = new TestHarness();
        var report = await NewService(h).SyncAsync();
        Assert.Equal(0, report.Fetched);
    }

    [Fact]
    public async Task New_document_is_inserted()
    {
        using var h = new TestHarness();
        var staged = new Document
        {
            Title = "BRK-100 WI",
            DocumentType = DocumentType.WorkInstruction,
            PartNumber = "BRK-100",
            Revision = "B",
            IsReleased = true,
            UrlOrPath = "/docs/BRK-100_B_wi.pdf"
        };

        var report = await NewService(h, new StubConnector(staged)).SyncAsync();

        Assert.Equal(1, report.Inserted);
        Assert.Equal(0, report.Updated);
        Assert.Single(h.Db.Documents);
    }

    [Fact]
    public async Task Existing_document_is_updated_by_path()
    {
        using var h = new TestHarness();
        h.Db.Documents.Add(new Document
        {
            Title = "Old title",
            DocumentType = DocumentType.WorkInstruction,
            PartNumber = "BRK-100",
            Revision = "A",
            IsReleased = true,
            UrlOrPath = "/docs/same-path.pdf"
        });
        await h.Db.SaveChangesAsync();

        var staged = new Document
        {
            Title = "New title",
            DocumentType = DocumentType.Drawing,         // type changed
            PartNumber = "BRK-100",
            Revision = "B",                              // rev changed
            IsReleased = true,
            UrlOrPath = "/docs/same-path.pdf"            // same path → matches
        };

        var report = await NewService(h, new StubConnector(staged)).SyncAsync();

        Assert.Equal(0, report.Inserted);
        Assert.Equal(1, report.Updated);
        var saved = h.Db.Documents.Single();
        Assert.Equal("New title", saved.Title);
        Assert.Equal(DocumentType.Drawing, saved.DocumentType);
        Assert.Equal("B", saved.Revision);
    }

    [Fact]
    public async Task Document_without_path_is_skipped_with_reason()
    {
        using var h = new TestHarness();
        var staged = new Document
        {
            Title = "Pathless doc",
            DocumentType = DocumentType.Other,
            UrlOrPath = ""
        };
        var report = await NewService(h, new StubConnector(staged)).SyncAsync();
        Assert.Equal(0, report.Inserted);
        Assert.Equal(1, report.Skipped);
        Assert.Single(report.SkipReasons);
        Assert.Empty(h.Db.Documents);
    }

    private sealed class StubConnector : IExternalDocumentConnector
    {
        private readonly Document[] _docs;
        public StubConnector(params Document[] docs) => _docs = docs;
        public async IAsyncEnumerable<Document> FetchDocumentsAsync(
            DateTime? changedSinceUtc = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var d in _docs)
            {
                ct.ThrowIfCancellationRequested();
                yield return d;
                await Task.Yield();
            }
        }
    }
}
