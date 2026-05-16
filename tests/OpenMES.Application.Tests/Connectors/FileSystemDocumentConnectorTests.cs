using Microsoft.Extensions.Logging.Abstractions;
using OpenMES.Domain.Enums;
using OpenMES.Infrastructure.Connectors;

namespace OpenMES.Application.Tests.Connectors;

public class FileSystemDocumentConnectorTests : IDisposable
{
    private readonly string _root;

    public FileSystemDocumentConnectorTests()
    {
        _root = Path.Combine(Path.GetTempPath(), $"openmes-docs-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* test cleanup is best-effort */ }
    }

    private void Touch(string filename) => File.WriteAllText(Path.Combine(_root, filename), "");

    private FileSystemDocumentConnector NewConnector(string? root)
        => new(new OpenMesConnectorOptions { FileSystemDocumentsRoot = root },
               NullLogger<FileSystemDocumentConnector>.Instance);

    [Fact]
    public async Task Returns_empty_when_root_not_configured()
    {
        var c = NewConnector(null);
        var list = new List<Domain.Entities.Document>();
        await foreach (var d in c.FetchDocumentsAsync()) list.Add(d);
        Assert.Empty(list);
    }

    [Fact]
    public async Task Returns_empty_when_root_missing()
    {
        var c = NewConnector("/tmp/nope-" + Guid.NewGuid().ToString("N"));
        var list = new List<Domain.Entities.Document>();
        await foreach (var d in c.FetchDocumentsAsync()) list.Add(d);
        Assert.Empty(list);
    }

    [Fact]
    public async Task Parses_each_supported_filename_convention()
    {
        Touch("BRK-100_B.pdf");
        Touch("BRK-100_B_wi.pdf");
        Touch("BRK-100_B_op_OP020_setup.pdf");
        Touch("ASM-900_D_procedure.pdf");
        Touch("resource_CNC-01_safety.pdf");
        Touch("random-unrecognised-file.txt");

        var c = NewConnector(_root);
        var docs = new List<Domain.Entities.Document>();
        await foreach (var d in c.FetchDocumentsAsync()) docs.Add(d);

        Assert.Equal(5, docs.Count);

        var drawing = docs.Single(d => d.UrlOrPath.EndsWith("BRK-100_B.pdf"));
        Assert.Equal(DocumentType.Drawing, drawing.DocumentType);
        Assert.Equal("BRK-100", drawing.PartNumber);
        Assert.Equal("B", drawing.Revision);
        Assert.Null(drawing.OperationCode);
        Assert.True(drawing.IsReleased);

        var wi = docs.Single(d => d.UrlOrPath.EndsWith("BRK-100_B_wi.pdf"));
        Assert.Equal(DocumentType.WorkInstruction, wi.DocumentType);

        var setup = docs.Single(d => d.UrlOrPath.EndsWith("BRK-100_B_op_OP020_setup.pdf"));
        Assert.Equal(DocumentType.SetupSheet, setup.DocumentType);
        Assert.Equal("OP020", setup.OperationCode);

        var proc = docs.Single(d => d.UrlOrPath.EndsWith("ASM-900_D_procedure.pdf"));
        Assert.Equal(DocumentType.Procedure, proc.DocumentType);

        var safety = docs.Single(d => d.UrlOrPath.EndsWith("resource_CNC-01_safety.pdf"));
        Assert.Equal(DocumentType.Safety, safety.DocumentType);
        Assert.Equal("CNC-01", safety.ResourceCode);
        Assert.Null(safety.PartNumber);
    }

    [Fact]
    public async Task Honours_changedSinceUtc_to_skip_old_files()
    {
        Touch("BRK-100_B_wi.pdf");
        var oldPath = Path.Combine(_root, "BRK-100_B_wi.pdf");
        File.SetLastWriteTimeUtc(oldPath, new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        Touch("SHF-220_C_wi.pdf");

        var c = NewConnector(_root);
        var docs = new List<Domain.Entities.Document>();
        await foreach (var d in c.FetchDocumentsAsync(changedSinceUtc: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)))
        {
            docs.Add(d);
        }

        Assert.Single(docs);
        Assert.Contains("SHF-220", docs[0].UrlOrPath);
    }
}
