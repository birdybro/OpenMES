using OpenMES.Application.Quality;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;

namespace OpenMES.Application.Tests.Quality;

public class QualityServiceTests
{
    /// <summary>Seeds one job + one op with two checks, returns (jobId, numericCheckId, passFailCheckId, visualCheckId).</summary>
    private static async Task<(int jobId, int numericId, int passFailId, int visualId, int otherJobId)> Seed(TestHarness h)
    {
        var part = new Part { PartNumber = "P1" };
        var rev = new PartRevision { Part = part, Revision = "A", IsCurrent = true };
        var op = new Operation { PartRevision = rev, OperationCode = "OP010", Description = "Inspect", Sequence = 10 };
        rev.Operations.Add(op);

        // Second part + job to verify scoping.
        var otherPart = new Part { PartNumber = "P2" };
        var otherRev = new PartRevision { Part = otherPart, Revision = "A", IsCurrent = true };
        var otherOp = new Operation { PartRevision = otherRev, OperationCode = "OP010", Description = "Other", Sequence = 10 };
        otherRev.Operations.Add(otherOp);

        var job = new Job { JobNumber = "J1", PartRevision = rev, QuantityOrdered = 1, Status = JobStatus.Released };
        var otherJob = new Job { JobNumber = "J2", PartRevision = otherRev, QuantityOrdered = 1, Status = JobStatus.Released };

        var numeric = new QualityCheck { Operation = op, Title = "OD", CheckType = QualityCheckType.Numeric, MinValue = 9.9m, MaxValue = 10.1m, Unit = "mm", Required = true };
        var passFail = new QualityCheck { Operation = op, Title = "Torque", CheckType = QualityCheckType.PassFail, Required = true };
        var visual = new QualityCheck { Operation = op, Title = "Finish", CheckType = QualityCheckType.Visual };
        var otherCheck = new QualityCheck { Operation = otherOp, Title = "Other check", CheckType = QualityCheckType.Visual };

        h.Db.AddRange(part, rev, op, otherPart, otherRev, otherOp, job, otherJob, numeric, passFail, visual, otherCheck);
        await h.Db.SaveChangesAsync();

        return (job.Id, numeric.Id, passFail.Id, visual.Id, otherJob.Id);
    }

    [Fact]
    public async Task ListChecksForJob_returns_checks_scoped_to_the_jobs_part_revision()
    {
        using var h = new TestHarness();
        var (jobId, _, _, _, _) = await Seed(h);

        var checks = await h.Quality.ListChecksForJobAsync(jobId);

        Assert.Equal(3, checks.Count);
        Assert.All(checks, c => Assert.Equal("OP010", c.Operation!.OperationCode));
        Assert.DoesNotContain(checks, c => c.Title == "Other check");
    }

    [Fact]
    public async Task ListChecksForJob_returns_empty_for_unknown_job()
    {
        using var h = new TestHarness();
        var checks = await h.Quality.ListChecksForJobAsync(99999);
        Assert.Empty(checks);
    }

    [Fact]
    public async Task Numeric_value_within_range_passes()
    {
        using var h = new TestHarness();
        var (jobId, numericId, _, _, _) = await Seed(h);
        var r = await h.Quality.RecordAsync(new QualityResultInput(numericId, jobId, NumericValue: 10.0m));
        Assert.True(r.Success);
        Assert.True(r.Result!.Pass);
        Assert.Equal(10.0m, r.Result.NumericValue);
    }

    [Theory]
    [InlineData(9.0)]    // below min
    [InlineData(10.5)]   // above max
    public async Task Numeric_value_outside_range_fails(decimal value)
    {
        using var h = new TestHarness();
        var (jobId, numericId, _, _, _) = await Seed(h);
        var r = await h.Quality.RecordAsync(new QualityResultInput(numericId, jobId, NumericValue: value));
        Assert.True(r.Success);            // recording succeeded
        Assert.False(r.Result!.Pass);      // but the value failed
    }

    [Fact]
    public async Task Numeric_check_without_a_value_is_rejected()
    {
        using var h = new TestHarness();
        var (jobId, numericId, _, _, _) = await Seed(h);
        var r = await h.Quality.RecordAsync(new QualityResultInput(numericId, jobId));
        Assert.False(r.Success);
        Assert.Contains("numeric value", r.FailureReason);
    }

    [Fact]
    public async Task PassFail_requires_explicit_selection()
    {
        using var h = new TestHarness();
        var (jobId, _, passFailId, _, _) = await Seed(h);

        var without = await h.Quality.RecordAsync(new QualityResultInput(passFailId, jobId));
        Assert.False(without.Success);

        var pass = await h.Quality.RecordAsync(new QualityResultInput(passFailId, jobId, PassOverride: true));
        Assert.True(pass.Success);
        Assert.True(pass.Result!.Pass);

        var fail = await h.Quality.RecordAsync(new QualityResultInput(passFailId, jobId, PassOverride: false));
        Assert.True(fail.Success);
        Assert.False(fail.Result!.Pass);
    }

    [Fact]
    public async Task Visual_check_passes_by_default()
    {
        using var h = new TestHarness();
        var (jobId, _, _, visualId, _) = await Seed(h);
        var r = await h.Quality.RecordAsync(new QualityResultInput(visualId, jobId));
        Assert.True(r.Success);
        Assert.True(r.Result!.Pass);
    }

    [Fact]
    public async Task Recording_emits_a_QualityCheckCompleted_event_with_pass_status_in_notes()
    {
        using var h = new TestHarness();
        var (jobId, numericId, _, _, _) = await Seed(h);

        var ok = await h.Quality.RecordAsync(new QualityResultInput(numericId, jobId, NumericValue: 10.0m));
        Assert.True(ok.Success);
        var fail = await h.Quality.RecordAsync(new QualityResultInput(numericId, jobId, NumericValue: 99));
        Assert.True(fail.Success);

        var events = h.Db.ProductionEvents
            .Where(e => e.EventType == ProductionEventType.QualityCheckCompleted)
            .OrderBy(e => e.Id)
            .ToList();
        Assert.Equal(2, events.Count);
        Assert.Contains("PASS", events[0].Notes);
        Assert.Null(events[0].ReasonCode);
        Assert.Contains("FAIL", events[1].Notes);
        Assert.Equal("QC-FAIL", events[1].ReasonCode);
    }

    [Fact]
    public async Task Unknown_job_or_check_is_rejected()
    {
        using var h = new TestHarness();
        var (jobId, numericId, _, _, _) = await Seed(h);

        var badJob = await h.Quality.RecordAsync(new QualityResultInput(numericId, JobId: 99999, NumericValue: 10));
        Assert.False(badJob.Success);

        var badCheck = await h.Quality.RecordAsync(new QualityResultInput(QualityCheckId: 99999, jobId, NumericValue: 10));
        Assert.False(badCheck.Success);
    }

    [Fact]
    public async Task ListResultsForJob_returns_results_newest_first_and_only_for_that_job()
    {
        using var h = new TestHarness();
        var (jobId, numericId, _, _, otherJobId) = await Seed(h);

        await h.Quality.RecordAsync(new QualityResultInput(numericId, jobId, NumericValue: 10));
        h.Clock.Advance(TimeSpan.FromMinutes(1));
        await h.Quality.RecordAsync(new QualityResultInput(numericId, jobId, NumericValue: 10.05m));

        var listed = await h.Quality.ListResultsForJobAsync(jobId);
        Assert.Equal(2, listed.Count);
        Assert.True(listed[0].RecordedUtc > listed[1].RecordedUtc);

        var noneForOther = await h.Quality.ListResultsForJobAsync(otherJobId);
        Assert.Empty(noneForOther);
    }

    [Fact]
    public async Task PassOverride_takes_precedence_over_numeric_range()
    {
        using var h = new TestHarness();
        var (jobId, numericId, _, _, _) = await Seed(h);
        // Value would normally fail (outside 9.9 … 10.1), but operator override forces a pass.
        var r = await h.Quality.RecordAsync(new QualityResultInput(numericId, jobId, NumericValue: 50, PassOverride: true));
        Assert.True(r.Success);
        Assert.True(r.Result!.Pass);
    }
}
