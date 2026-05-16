using Microsoft.EntityFrameworkCore;
using OpenMES.Domain.Entities;

namespace OpenMES.Infrastructure.Persistence;

public class OpenMesDbContext : DbContext
{
    public OpenMesDbContext(DbContextOptions<OpenMesDbContext> options) : base(options) { }

    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PartRevision> PartRevisions => Set<PartRevision>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ResourceScheduleEntry> ResourceSchedule => Set<ResourceScheduleEntry>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentLink> DocumentLinks => Set<DocumentLink>();
    public DbSet<MaterialLot> MaterialLots => Set<MaterialLot>();
    public DbSet<JobMaterialIssue> JobMaterialIssues => Set<JobMaterialIssue>();
    public DbSet<ProductionEvent> ProductionEvents => Set<ProductionEvent>();
    public DbSet<QualityCheck> QualityChecks => Set<QualityCheck>();
    public DbSet<QualityResult> QualityResults => Set<QualityResult>();
    public DbSet<ScanEvent> ScanEvents => Set<ScanEvent>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.ApplyConfigurationsFromAssembly(typeof(OpenMesDbContext).Assembly);
    }
}
