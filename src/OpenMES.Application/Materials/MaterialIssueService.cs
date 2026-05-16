using Microsoft.EntityFrameworkCore;
using OpenMES.Application.Abstractions;
using OpenMES.Application.Production;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Materials;

public sealed record MaterialIssueResult(
    bool Success,
    string? FailureReason,
    JobMaterialIssue? Issue,
    ProductionEvent? Event)
{
    public static MaterialIssueResult Ok(JobMaterialIssue issue, ProductionEvent evt) => new(true, null, issue, evt);
    public static MaterialIssueResult Fail(string reason) => new(false, reason, null, null);
}

public sealed class MaterialIssueService
{
    private readonly IOpenMesDb _db;
    private readonly IClock _clock;
    private readonly ProductionEventService _events;
    private readonly IEnumerable<IMaterialValidationRule> _rules;

    public MaterialIssueService(
        IOpenMesDb db,
        IClock clock,
        ProductionEventService events,
        IEnumerable<IMaterialValidationRule> rules)
    {
        _db = db;
        _clock = clock;
        _events = events;
        _rules = rules;
    }

    public Task<MaterialLot?> FindLotAsync(string lotCode, CancellationToken ct = default)
        => _db.MaterialLots.FirstOrDefaultAsync(l => l.LotCode == lotCode, ct);

    public Task<List<JobMaterialIssue>> ListForJobAsync(int jobId, CancellationToken ct = default)
        => _db.JobMaterialIssues
            .Include(i => i.MaterialLot)
            .Where(i => i.JobId == jobId)
            .OrderByDescending(i => i.IssuedUtc)
            .ToListAsync(ct);

    public async Task<MaterialIssueResult> IssueAsync(
        int jobId,
        string lotCode,
        decimal quantity,
        int? userId = null,
        CancellationToken ct = default)
    {
        if (quantity <= 0)
        {
            return MaterialIssueResult.Fail("Quantity must be greater than zero.");
        }

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null) return MaterialIssueResult.Fail($"Job {jobId} not found.");

        var lot = await _db.MaterialLots.FirstOrDefaultAsync(l => l.LotCode == lotCode, ct);
        if (lot is null) return MaterialIssueResult.Fail($"Lot '{lotCode}' not found.");

        if (lot.Status != MaterialLotStatus.Available)
        {
            return MaterialIssueResult.Fail($"Lot '{lot.LotCode}' is {lot.Status}, not Available.");
        }
        if (lot.QuantityOnHand < quantity)
        {
            return MaterialIssueResult.Fail($"Lot has {lot.QuantityOnHand} {lot.UnitOfMeasure} on hand; requested {quantity}.");
        }

        var ctx = new MaterialIssueContext(job, lot, quantity);
        foreach (var rule in _rules)
        {
            var v = await rule.ValidateAsync(ctx, ct);
            if (!v.IsAllowed) return MaterialIssueResult.Fail(v.Reason ?? "Disallowed by validation rule.");
        }

        var issue = new JobMaterialIssue
        {
            JobId = jobId,
            MaterialLotId = lot.Id,
            Quantity = quantity,
            IssuedUtc = _clock.UtcNow,
            IssuedByUserId = userId
        };
        _db.Add(issue);

        lot.QuantityOnHand -= quantity;
        if (lot.QuantityOnHand <= 0)
        {
            lot.QuantityOnHand = 0;
            lot.Status = MaterialLotStatus.Consumed;
        }

        await _db.SaveChangesAsync(ct);

        var evt = await _events.RecordAsync(new ProductionEventInput(
            ProductionEventType.MaterialIssued,
            JobId: jobId,
            UserId: userId,
            Quantity: quantity,
            Notes: $"Lot {lot.LotCode} → Job {job.JobNumber}"), ct);

        return MaterialIssueResult.Ok(issue, evt);
    }
}
