using OpenMES.Application.Abstractions;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence;

/// <summary>
/// Bridges the application's <see cref="IOpenMesDb"/> abstraction onto the
/// concrete EF Core <see cref="OpenMesDbContext"/>.
/// </summary>
public sealed class OpenMesDbAdapter : IOpenMesDb
{
    private readonly OpenMesDbContext _ctx;

    public OpenMesDbAdapter(OpenMesDbContext ctx) => _ctx = ctx;

    public IQueryable<Job> Jobs => _ctx.Jobs;
    public IQueryable<Part> Parts => _ctx.Parts;
    public IQueryable<PartRevision> PartRevisions => _ctx.PartRevisions;
    public IQueryable<Operation> Operations => _ctx.Operations;
    public IQueryable<Resource> Resources => _ctx.Resources;
    public IQueryable<ResourceScheduleEntry> ResourceSchedule => _ctx.ResourceSchedule;
    public IQueryable<Document> Documents => _ctx.Documents;
    public IQueryable<DocumentLink> DocumentLinks => _ctx.DocumentLinks;
    public IQueryable<MaterialLot> MaterialLots => _ctx.MaterialLots;
    public IQueryable<JobMaterialIssue> JobMaterialIssues => _ctx.JobMaterialIssues;
    public IQueryable<ProductionEvent> ProductionEvents => _ctx.ProductionEvents;
    public IQueryable<QualityCheck> QualityChecks => _ctx.QualityChecks;
    public IQueryable<QualityResult> QualityResults => _ctx.QualityResults;
    public IQueryable<ScanEvent> ScanEvents => _ctx.ScanEvents;
    public IQueryable<User> Users => _ctx.Users;

    public void Add<T>(T entity) where T : class => _ctx.Add(entity);
    public void Update<T>(T entity) where T : class => _ctx.Update(entity);
    public void Remove<T>(T entity) where T : class => _ctx.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _ctx.SaveChangesAsync(cancellationToken);
}
