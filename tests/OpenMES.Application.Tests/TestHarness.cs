using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OpenMES.Application.Abstractions;
using OpenMES.Application.Documents;
using OpenMES.Application.Jobs;
using OpenMES.Application.Materials;
using OpenMES.Application.Production;
using OpenMES.Application.Scanning;
using OpenMES.Infrastructure.Persistence;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Tests;

/// <summary>
/// Wires an in-memory EF context, the real <see cref="OpenMesDbAdapter"/>, and
/// the application services so each test gets a fresh, isolated graph.
/// </summary>
internal sealed class TestHarness : IDisposable
{
    public OpenMesDbContext Db { get; }
    public IOpenMesDb Data { get; }
    public TestClock Clock { get; } = new();
    public ProductionEventService Events { get; }
    public JobService Jobs { get; }
    public DocumentService Documents { get; }
    public ScanService Scans { get; }
    public MaterialIssueService Materials { get; }
    public DefaultDocumentResolver Resolver { get; }

    public TestHarness(
        IEnumerable<IBarcodeParser>? parsers = null,
        IEnumerable<IMaterialValidationRule>? rules = null,
        IEnumerable<IProductionEventSink>? sinks = null,
        IEnumerable<IDocumentResolver>? resolvers = null)
    {
        var opts = new DbContextOptionsBuilder<OpenMesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        Db = new OpenMesDbContext(opts);
        Data = new OpenMesDbAdapter(Db);

        Events = new ProductionEventService(
            Data, Clock,
            sinks ?? Array.Empty<IProductionEventSink>(),
            NullLogger<ProductionEventService>.Instance);

        Jobs = new JobService(Data, Clock, Events);
        Resolver = new DefaultDocumentResolver(Data);
        Documents = new DocumentService(
            Data,
            resolvers ?? new IDocumentResolver[] { Resolver },
            Events);
        Scans = new ScanService(
            parsers ?? Array.Empty<IBarcodeParser>(),
            Data, Clock, Events);
        Materials = new MaterialIssueService(
            Data, Clock, Events,
            rules ?? Array.Empty<IMaterialValidationRule>());
    }

    public void Dispose() => Db.Dispose();
}

internal sealed class TestClock : IClock
{
    public DateTime UtcNow { get; set; } = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    public void Advance(TimeSpan ts) => UtcNow = UtcNow.Add(ts);
}
