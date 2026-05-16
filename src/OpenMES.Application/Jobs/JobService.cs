using Microsoft.EntityFrameworkCore;
using OpenMES.Application.Abstractions;
using OpenMES.Application.Production;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;

namespace OpenMES.Application.Jobs;

public sealed class JobService
{
    private readonly IOpenMesDb _db;
    private readonly IClock _clock;
    private readonly ProductionEventService _events;

    public JobService(IOpenMesDb db, IClock clock, ProductionEventService events)
    {
        _db = db;
        _clock = clock;
        _events = events;
    }

    public Task<List<Job>> ListAsync(CancellationToken ct = default)
        => _db.Jobs
            .Include(j => j.PartRevision)!.ThenInclude(r => r!.Part)
            .Include(j => j.Resource)
            .OrderBy(j => j.Status == JobStatus.Completed || j.Status == JobStatus.Cancelled)
            .ThenBy(j => j.DueUtc ?? DateTime.MaxValue)
            .ThenBy(j => j.JobNumber)
            .ToListAsync(ct);

    public Task<Job?> GetAsync(int id, CancellationToken ct = default)
        => _db.Jobs
            .Include(j => j.PartRevision)!.ThenInclude(r => r!.Part)
            .Include(j => j.PartRevision)!.ThenInclude(r => r!.Operations)
            .Include(j => j.Resource)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

    public Task<List<Job>> ListForResourceAsync(int resourceId, CancellationToken ct = default)
        => _db.Jobs
            .Include(j => j.PartRevision)!.ThenInclude(r => r!.Part)
            .Where(j => j.ResourceId == resourceId)
            .OrderBy(j => j.Status == JobStatus.Completed || j.Status == JobStatus.Cancelled)
            .ThenBy(j => j.DueUtc ?? DateTime.MaxValue)
            .ToListAsync(ct);

    public async Task<Job> ReleaseAsync(int jobId, int? userId = null, CancellationToken ct = default)
    {
        var job = await RequireJob(jobId, ct);
        if (job.Status == JobStatus.Created)
        {
            job.Status = JobStatus.Released;
            job.ReleasedUtc = _clock.UtcNow;
            await _events.RecordAsync(new ProductionEventInput(
                ProductionEventType.JobReleased, JobId: job.Id, UserId: userId), ct);
        }
        return job;
    }

    public async Task<Job> StartAsync(int jobId, int? userId = null, CancellationToken ct = default)
    {
        var job = await RequireJob(jobId, ct);
        if (job.Status is JobStatus.Created or JobStatus.Released or JobStatus.Paused)
        {
            var wasPaused = job.Status == JobStatus.Paused;
            job.Status = JobStatus.InProgress;
            job.StartedUtc ??= _clock.UtcNow;
            await _events.RecordAsync(new ProductionEventInput(
                wasPaused ? ProductionEventType.JobResumed : ProductionEventType.JobStarted,
                JobId: job.Id, ResourceId: job.ResourceId, UserId: userId), ct);
        }
        return job;
    }

    public async Task<Job> PauseAsync(int jobId, string? reason = null, int? userId = null, CancellationToken ct = default)
    {
        var job = await RequireJob(jobId, ct);
        if (job.Status == JobStatus.InProgress)
        {
            job.Status = JobStatus.Paused;
            await _events.RecordAsync(new ProductionEventInput(
                ProductionEventType.JobPaused, JobId: job.Id, ResourceId: job.ResourceId,
                UserId: userId, ReasonCode: reason), ct);
        }
        return job;
    }

    public async Task<Job> CompleteAsync(int jobId, int? userId = null, CancellationToken ct = default)
    {
        var job = await RequireJob(jobId, ct);
        if (job.Status != JobStatus.Completed && job.Status != JobStatus.Cancelled)
        {
            job.Status = JobStatus.Completed;
            job.CompletedUtc = _clock.UtcNow;
            await _events.RecordAsync(new ProductionEventInput(
                ProductionEventType.JobCompleted, JobId: job.Id, ResourceId: job.ResourceId, UserId: userId), ct);
        }
        return job;
    }

    private async Task<Job> RequireJob(int id, CancellationToken ct)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id, ct)
            ?? throw new InvalidOperationException($"Job {id} not found.");
        return job;
    }
}
