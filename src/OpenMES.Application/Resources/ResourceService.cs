using Microsoft.EntityFrameworkCore;
using OpenMES.Application.Abstractions;
using OpenMES.Domain.Entities;

namespace OpenMES.Application.Resources;

public sealed class ResourceService
{
    private readonly IOpenMesDb _db;

    public ResourceService(IOpenMesDb db) => _db = db;

    public Task<List<Resource>> ListAsync(CancellationToken ct = default)
        => _db.Resources
            .Where(r => r.IsActive)
            .OrderBy(r => r.Code)
            .ToListAsync(ct);

    public Task<Resource?> GetAsync(int id, CancellationToken ct = default)
        => _db.Resources.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<ResourceScheduleEntry>> GetUpcomingScheduleAsync(int resourceId, DateTime fromUtc, CancellationToken ct = default)
        => _db.ResourceSchedule
            .Include(s => s.Job)!.ThenInclude(j => j!.PartRevision)!.ThenInclude(r => r!.Part)
            .Where(s => s.ResourceId == resourceId && s.PlannedEndUtc >= fromUtc)
            .OrderBy(s => s.PlannedStartUtc)
            .ToListAsync(ct);
}
