using OpenMES.Application.Tests;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Tests.Materials;

public class MaterialIssueServiceTests
{
    private static async Task<(int jobId, MaterialLot lot)> Seed(TestHarness h, MaterialLotStatus status = MaterialLotStatus.Available, decimal onHand = 100)
    {
        var part = new Part { PartNumber = "P1" };
        var rev = new PartRevision { Part = part, Revision = "A", IsCurrent = true };
        var job = new Job { JobNumber = "J1", PartRevision = rev, QuantityOrdered = 50, Status = JobStatus.Released };
        var lot = new MaterialLot { LotCode = "LOT-1", PartNumber = "P1", QuantityOnHand = onHand, Status = status };
        h.Db.Add(part); h.Db.Add(rev); h.Db.Add(job); h.Db.Add(lot);
        await h.Db.SaveChangesAsync();
        return (job.Id, lot);
    }

    [Fact]
    public async Task Issuing_a_valid_lot_decrements_quantity_and_emits_event()
    {
        using var h = new TestHarness();
        var (jobId, lot) = await Seed(h, onHand: 100);

        var r = await h.Materials.IssueAsync(jobId, lot.LotCode, 30);

        Assert.True(r.Success);
        Assert.NotNull(r.Issue);
        Assert.NotNull(r.Event);
        Assert.Equal(70, h.Db.MaterialLots.Single().QuantityOnHand);
        Assert.Equal(ProductionEventType.MaterialIssued, h.Db.ProductionEvents.Single().EventType);
    }

    [Fact]
    public async Task Quantity_must_be_positive()
    {
        using var h = new TestHarness();
        var (jobId, lot) = await Seed(h);
        var r = await h.Materials.IssueAsync(jobId, lot.LotCode, 0);
        Assert.False(r.Success);
        Assert.Contains("greater than zero", r.FailureReason);
    }

    [Fact]
    public async Task Unknown_lot_is_rejected()
    {
        using var h = new TestHarness();
        var (jobId, _) = await Seed(h);
        var r = await h.Materials.IssueAsync(jobId, "NOT-A-LOT", 5);
        Assert.False(r.Success);
        Assert.Contains("not found", r.FailureReason);
    }

    [Fact]
    public async Task Unknown_job_is_rejected()
    {
        using var h = new TestHarness();
        var (_, lot) = await Seed(h);
        var r = await h.Materials.IssueAsync(jobId: 99999, lot.LotCode, 1);
        Assert.False(r.Success);
        Assert.Contains("Job", r.FailureReason);
    }

    [Fact]
    public async Task Lot_on_hold_is_rejected()
    {
        using var h = new TestHarness();
        var (jobId, lot) = await Seed(h, status: MaterialLotStatus.OnHold);
        var r = await h.Materials.IssueAsync(jobId, lot.LotCode, 1);
        Assert.False(r.Success);
        Assert.Contains("OnHold", r.FailureReason);
    }

    [Fact]
    public async Task Issuing_more_than_on_hand_is_rejected()
    {
        using var h = new TestHarness();
        var (jobId, lot) = await Seed(h, onHand: 10);
        var r = await h.Materials.IssueAsync(jobId, lot.LotCode, 50);
        Assert.False(r.Success);
        Assert.Contains("on hand", r.FailureReason);
    }

    [Fact]
    public async Task Issuing_full_quantity_marks_lot_consumed()
    {
        using var h = new TestHarness();
        var (jobId, lot) = await Seed(h, onHand: 10);
        var r = await h.Materials.IssueAsync(jobId, lot.LotCode, 10);
        Assert.True(r.Success);
        var refreshed = h.Db.MaterialLots.Single();
        Assert.Equal(0, refreshed.QuantityOnHand);
        Assert.Equal(MaterialLotStatus.Consumed, refreshed.Status);
    }

    [Fact]
    public async Task Validation_rule_can_deny_an_issue()
    {
        using var h = new TestHarness(rules: new[] { new AlwaysDenyRule("nope") });
        var (jobId, lot) = await Seed(h);
        var r = await h.Materials.IssueAsync(jobId, lot.LotCode, 5);
        Assert.False(r.Success);
        Assert.Equal("nope", r.FailureReason);
    }

    private sealed class AlwaysDenyRule : IMaterialValidationRule
    {
        private readonly string _reason;
        public AlwaysDenyRule(string reason) => _reason = reason;
        public Task<MaterialValidationResult> ValidateAsync(MaterialIssueContext _, CancellationToken __ = default)
            => Task.FromResult(MaterialValidationResult.Deny(_reason));
    }
}
