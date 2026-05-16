using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenMES.Application.Abstractions;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Production;

public sealed record ProductionEventInput(
    ProductionEventType EventType,
    int? JobId = null,
    int? ResourceId = null,
    int? OperationId = null,
    int? UserId = null,
    decimal? Quantity = null,
    string? ReasonCode = null,
    string? Notes = null,
    string? RawScanValue = null);

/// <summary>
/// Writes append-only <see cref="ProductionEvent"/> records and fans them out
/// to any registered <see cref="IProductionEventSink"/>s. Aggregate caches on
/// <see cref="Job"/> (QuantityGood / QuantityScrap) are advanced here too.
/// </summary>
public sealed class ProductionEventService
{
    private readonly IOpenMesDb _db;
    private readonly IClock _clock;
    private readonly IEnumerable<IProductionEventSink> _sinks;
    private readonly ILogger<ProductionEventService> _log;

    public ProductionEventService(
        IOpenMesDb db,
        IClock clock,
        IEnumerable<IProductionEventSink> sinks,
        ILogger<ProductionEventService> log)
    {
        _db = db;
        _clock = clock;
        _sinks = sinks;
        _log = log;
    }

    public async Task<ProductionEvent> RecordAsync(ProductionEventInput input, CancellationToken ct = default)
    {
        var evt = new ProductionEvent
        {
            EventType = input.EventType,
            JobId = input.JobId,
            ResourceId = input.ResourceId,
            OperationId = input.OperationId,
            UserId = input.UserId,
            Quantity = input.Quantity,
            ReasonCode = input.ReasonCode,
            Notes = input.Notes,
            RawScanValue = input.RawScanValue,
            TimestampUtc = _clock.UtcNow
        };
        _db.Add(evt);

        if (input.JobId is int jobId)
        {
            await ApplyToJobCache(jobId, input, ct);
        }

        await _db.SaveChangesAsync(ct);

        foreach (var sink in _sinks)
        {
            try
            {
                await sink.HandleAsync(evt, ct);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Production event sink {Sink} threw — swallowed.", sink.GetType().Name);
            }
        }

        return evt;
    }

    public Task<ProductionEvent> ReportGoodAsync(int jobId, decimal qty, int? userId, int? operationId = null, int? resourceId = null, CancellationToken ct = default)
        => RecordAsync(new ProductionEventInput(
            ProductionEventType.GoodQuantityReported, jobId, resourceId, operationId, userId, qty), ct);

    public Task<ProductionEvent> ReportScrapAsync(int jobId, decimal qty, string? reason, int? userId, int? operationId = null, int? resourceId = null, CancellationToken ct = default)
        => RecordAsync(new ProductionEventInput(
            ProductionEventType.ScrapQuantityReported, jobId, resourceId, operationId, userId, qty, reason), ct);

    public Task<ProductionEvent> StartDowntimeAsync(int? jobId, int resourceId, string reason, int? userId, CancellationToken ct = default)
        => RecordAsync(new ProductionEventInput(
            ProductionEventType.DowntimeStarted, jobId, resourceId, UserId: userId, ReasonCode: reason), ct);

    public Task<ProductionEvent> EndDowntimeAsync(int? jobId, int resourceId, int? userId, CancellationToken ct = default)
        => RecordAsync(new ProductionEventInput(
            ProductionEventType.DowntimeEnded, jobId, resourceId, UserId: userId), ct);

    public Task<List<ProductionEvent>> ListForJobAsync(int jobId, int take = 200, CancellationToken ct = default)
        => _db.ProductionEvents
            .Where(e => e.JobId == jobId)
            .OrderByDescending(e => e.TimestampUtc)
            .ThenByDescending(e => e.Id)
            .Take(take)
            .ToListAsync(ct);

    private async Task ApplyToJobCache(int jobId, ProductionEventInput input, CancellationToken ct)
    {
        if (input.EventType is not ProductionEventType.GoodQuantityReported
            and not ProductionEventType.ScrapQuantityReported
            and not ProductionEventType.JobStarted
            and not ProductionEventType.JobCompleted)
        {
            return;
        }

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null) return;

        switch (input.EventType)
        {
            case ProductionEventType.GoodQuantityReported when input.Quantity is decimal good:
                job.QuantityGood += good;
                if (job.Status is JobStatus.Released or JobStatus.Created)
                {
                    job.Status = JobStatus.InProgress;
                    job.StartedUtc ??= _clock.UtcNow;
                }
                break;
            case ProductionEventType.ScrapQuantityReported when input.Quantity is decimal scrap:
                job.QuantityScrap += scrap;
                break;
            case ProductionEventType.JobStarted:
                job.StartedUtc ??= _clock.UtcNow;
                break;
            case ProductionEventType.JobCompleted:
                job.CompletedUtc ??= _clock.UtcNow;
                break;
        }
    }
}
