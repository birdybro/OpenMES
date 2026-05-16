using OpenMES.Domain.Entities;

namespace OpenMES.Application.Abstractions;

/// <summary>
/// Application-facing view of the OpenMES data store. Defined here so that
/// application services don't have to reference EF Core directly. Infrastructure
/// implements this on top of <c>OpenMesDbContext</c>.
/// </summary>
public interface IOpenMesDb
{
    IQueryable<Job> Jobs { get; }
    IQueryable<Part> Parts { get; }
    IQueryable<PartRevision> PartRevisions { get; }
    IQueryable<Operation> Operations { get; }
    IQueryable<Resource> Resources { get; }
    IQueryable<ResourceScheduleEntry> ResourceSchedule { get; }
    IQueryable<Document> Documents { get; }
    IQueryable<DocumentLink> DocumentLinks { get; }
    IQueryable<MaterialLot> MaterialLots { get; }
    IQueryable<JobMaterialIssue> JobMaterialIssues { get; }
    IQueryable<ProductionEvent> ProductionEvents { get; }
    IQueryable<QualityCheck> QualityChecks { get; }
    IQueryable<QualityResult> QualityResults { get; }
    IQueryable<ScanEvent> ScanEvents { get; }
    IQueryable<User> Users { get; }

    void Add<T>(T entity) where T : class;
    void Update<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
