using Microsoft.EntityFrameworkCore;
using OpenMES.Application.Abstractions;
using OpenMES.Application.Production;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;

namespace OpenMES.Application.Quality;

public sealed record QualityResultInput(
    int QualityCheckId,
    int JobId,
    decimal? NumericValue = null,
    string? TextValue = null,
    bool? PassOverride = null,
    int? UserId = null,
    string? Notes = null);

public sealed record QualityRecordResult(
    bool Success,
    string? FailureReason,
    QualityResult? Result,
    ProductionEvent? Event)
{
    public static QualityRecordResult Ok(QualityResult r, ProductionEvent e) => new(true, null, r, e);
    public static QualityRecordResult Fail(string reason) => new(false, reason, null, null);
}

public sealed class QualityService
{
    private readonly IOpenMesDb _db;
    private readonly IClock _clock;
    private readonly ProductionEventService _events;

    public QualityService(IOpenMesDb db, IClock clock, ProductionEventService events)
    {
        _db = db;
        _clock = clock;
        _events = events;
    }

    /// <summary>
    /// Quality checks defined on the operations of the job's part revision.
    /// </summary>
    public async Task<List<QualityCheck>> ListChecksForJobAsync(int jobId, CancellationToken ct = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null) return new();

        return await _db.QualityChecks
            .Include(c => c.Operation)
            .Where(c => c.Operation!.PartRevisionId == job.PartRevisionId)
            .OrderBy(c => c.Operation!.Sequence)
            .ThenBy(c => c.Title)
            .ToListAsync(ct);
    }

    public Task<List<QualityResult>> ListResultsForJobAsync(int jobId, CancellationToken ct = default)
        => _db.QualityResults
            .Include(r => r.QualityCheck)
            .Where(r => r.JobId == jobId)
            .OrderByDescending(r => r.RecordedUtc)
            .ToListAsync(ct);

    public async Task<QualityRecordResult> RecordAsync(QualityResultInput input, CancellationToken ct = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == input.JobId, ct);
        if (job is null) return QualityRecordResult.Fail($"Job {input.JobId} not found.");

        var check = await _db.QualityChecks
            .Include(c => c.Operation)
            .FirstOrDefaultAsync(c => c.Id == input.QualityCheckId, ct);
        if (check is null) return QualityRecordResult.Fail($"Quality check {input.QualityCheckId} not found.");

        var (pass, reason) = Evaluate(check, input);
        if (reason is not null) return QualityRecordResult.Fail(reason);

        var result = new QualityResult
        {
            QualityCheckId = check.Id,
            JobId = job.Id,
            NumericValue = input.NumericValue,
            TextValue = input.TextValue,
            Pass = pass,
            RecordedUtc = _clock.UtcNow,
            RecordedByUserId = input.UserId,
            Notes = input.Notes
        };
        _db.Add(result);
        await _db.SaveChangesAsync(ct);

        var evt = await _events.RecordAsync(new ProductionEventInput(
            ProductionEventType.QualityCheckCompleted,
            JobId: job.Id,
            OperationId: check.OperationId,
            UserId: input.UserId,
            Quantity: input.NumericValue,
            ReasonCode: pass ? null : "QC-FAIL",
            Notes: $"{check.Title} → {(pass ? "PASS" : "FAIL")}"), ct);

        return QualityRecordResult.Ok(result, evt);
    }

    /// <summary>
    /// Decides whether a recorded value passes the check. Returns
    /// <c>(pass, failureReason)</c> where a non-null reason means the
    /// submission itself was malformed (e.g. a numeric check with no value).
    /// </summary>
    internal static (bool Pass, string? Reason) Evaluate(QualityCheck check, QualityResultInput input)
    {
        if (input.PassOverride is bool overridden)
        {
            return (overridden, null);
        }

        switch (check.CheckType)
        {
            case QualityCheckType.Numeric:
                if (input.NumericValue is not decimal value)
                {
                    return (false, "Numeric check requires a numeric value.");
                }
                if (check.MinValue is decimal min && value < min) return (false, null);
                if (check.MaxValue is decimal max && value > max) return (false, null);
                return (true, null);

            case QualityCheckType.PassFail:
                return (false, "Pass/Fail check requires an explicit pass/fail selection.");

            case QualityCheckType.Visual:
            case QualityCheckType.Text:
                // No data to disprove a default pass — operator confirmation is
                // the whole point of these check types.
                return (true, null);

            default:
                return (true, null);
        }
    }
}
