using Microsoft.EntityFrameworkCore;
using OpenMES.Application.Abstractions;
using OpenMES.Application.Production;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Documents;

public sealed class DocumentService
{
    private readonly IOpenMesDb _db;
    private readonly IEnumerable<IDocumentResolver> _resolvers;
    private readonly ProductionEventService _events;

    public DocumentService(IOpenMesDb db, IEnumerable<IDocumentResolver> resolvers, ProductionEventService events)
    {
        _db = db;
        _resolvers = resolvers;
        _events = events;
    }

    public Task<List<Document>> ListAllAsync(CancellationToken ct = default)
        => _db.Documents
            .Include(d => d.Links)
            .OrderBy(d => d.IsObsolete)
            .ThenBy(d => d.PartNumber)
            .ThenBy(d => d.Title)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Document>> GetForJobAsync(int jobId, CancellationToken ct = default)
    {
        var job = await _db.Jobs
            .Include(j => j.PartRevision)!.ThenInclude(r => r!.Part)
            .Include(j => j.Resource)
            .FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job?.PartRevision?.Part is null) return Array.Empty<Document>();

        var ctx = new DocumentResolutionContext(
            jobId,
            job.PartRevision.Part.PartNumber,
            job.PartRevision.Revision,
            OperationCode: null,
            ResourceCode: job.Resource?.Code);

        var seen = new Dictionary<int, Document>();
        foreach (var resolver in _resolvers)
        {
            var docs = await resolver.ResolveAsync(ctx, ct);
            foreach (var d in docs)
            {
                seen[d.Id] = d;
            }
        }
        return seen.Values.ToList();
    }

    public async Task<ProductionEvent> RecordOpenedAsync(int jobId, int documentId, int? userId, CancellationToken ct = default)
        => await _events.RecordAsync(new ProductionEventInput(
            ProductionEventType.DocumentOpened,
            JobId: jobId,
            UserId: userId,
            Notes: $"DocumentId={documentId}"), ct);
}
