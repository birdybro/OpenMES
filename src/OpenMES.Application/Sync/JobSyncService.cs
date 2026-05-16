using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenMES.Application.Abstractions;
using OpenMES.Application.Production;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Sync;

/// <summary>
/// Pulls jobs from every registered <see cref="IExternalJobConnector"/> and
/// upserts them. Master-data lookups (PartRevision, Resource) happen here —
/// the connector contract only carries natural keys on the navigation
/// stubs. Missing master data is skipped with a reason rather than
/// auto-created, so ERP master-data problems stay visible.
/// </summary>
public sealed class JobSyncService
{
    private readonly IOpenMesDb _db;
    private readonly IClock _clock;
    private readonly IEnumerable<IExternalJobConnector> _connectors;
    private readonly ProductionEventService _events;
    private readonly ILogger<JobSyncService> _log;

    public JobSyncService(
        IOpenMesDb db,
        IClock clock,
        IEnumerable<IExternalJobConnector> connectors,
        ProductionEventService events,
        ILogger<JobSyncService> log)
    {
        _db = db;
        _clock = clock;
        _connectors = connectors;
        _events = events;
        _log = log;
    }

    public async Task<SyncReport> SyncAsync(DateTime? changedSinceUtc = null, CancellationToken ct = default)
    {
        var report = new SyncReport();

        foreach (var connector in _connectors)
        {
            await foreach (var staged in connector.FetchJobsAsync(changedSinceUtc, ct).WithCancellation(ct))
            {
                report.Fetched++;
                try
                {
                    await ApplyAsync(staged, report, ct);
                }
                catch (Exception ex)
                {
                    report.Errors++;
                    report.ErrorMessages.Add($"Job {staged.JobNumber}: {ex.Message}");
                    _log.LogError(ex, "Job sync failed for {JobNumber}.", staged.JobNumber);
                }
            }
        }

        report.FinishedUtc = _clock.UtcNow;
        return report;
    }

    private async Task ApplyAsync(Job staged, SyncReport report, CancellationToken ct)
    {
        var partNumber = staged.PartRevision?.Part?.PartNumber;
        var revision = staged.PartRevision?.Revision;
        if (string.IsNullOrEmpty(partNumber) || string.IsNullOrEmpty(revision))
        {
            report.Skipped++;
            report.SkipReasons.Add($"Job {staged.JobNumber}: missing part / revision in source.");
            return;
        }

        var rev = await _db.PartRevisions
            .Include(r => r.Part)
            .FirstOrDefaultAsync(r => r.Part!.PartNumber == partNumber && r.Revision == revision, ct);
        if (rev is null)
        {
            report.Skipped++;
            report.SkipReasons.Add($"Job {staged.JobNumber}: PartRevision {partNumber}/{revision} not found.");
            return;
        }

        int? resourceId = null;
        var resourceCode = staged.Resource?.Code;
        if (!string.IsNullOrEmpty(resourceCode))
        {
            var resource = await _db.Resources.FirstOrDefaultAsync(r => r.Code == resourceCode, ct);
            if (resource is null)
            {
                report.SkipReasons.Add($"Job {staged.JobNumber}: Resource '{resourceCode}' not found — job upserted without resource.");
            }
            else
            {
                resourceId = resource.Id;
            }
        }

        var existing = await _db.Jobs.FirstOrDefaultAsync(j => j.JobNumber == staged.JobNumber, ct);
        if (existing is null)
        {
            var fresh = new Job
            {
                JobNumber = staged.JobNumber,
                PartRevisionId = rev.Id,
                ResourceId = resourceId,
                QuantityOrdered = staged.QuantityOrdered,
                Status = JobStatus.Created,
                CreatedUtc = _clock.UtcNow,
                DueUtc = staged.DueUtc,
                Notes = staged.Notes
            };
            _db.Add(fresh);
            await _db.SaveChangesAsync(ct);
            await _events.RecordAsync(new ProductionEventInput(
                ProductionEventType.JobCreated, JobId: fresh.Id), ct);
            report.Inserted++;
        }
        else
        {
            // Don't churn fields the floor has been editing — only sync the
            // ones the ERP owns.
            existing.PartRevisionId = rev.Id;
            existing.ResourceId = resourceId ?? existing.ResourceId;
            existing.QuantityOrdered = staged.QuantityOrdered;
            existing.DueUtc = staged.DueUtc ?? existing.DueUtc;
            if (!string.IsNullOrWhiteSpace(staged.Notes)) existing.Notes = staged.Notes;
            await _db.SaveChangesAsync(ct);
            report.Updated++;
        }
    }
}
