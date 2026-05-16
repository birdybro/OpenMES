using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;

namespace OpenMES.Application.Tests.Jobs;

public class JobServiceTests
{
    private static Job NewJob() => new()
    {
        JobNumber = "J1",
        QuantityOrdered = 10,
        Status = JobStatus.Created
    };

    [Fact]
    public async Task Lifecycle_transitions_emit_expected_events()
    {
        using var h = new TestHarness();
        var job = NewJob();
        h.Db.Jobs.Add(job);
        await h.Db.SaveChangesAsync();

        await h.Jobs.ReleaseAsync(job.Id);
        await h.Jobs.StartAsync(job.Id);
        await h.Jobs.PauseAsync(job.Id, reason: "BREAK");
        await h.Jobs.StartAsync(job.Id);   // resume
        await h.Jobs.CompleteAsync(job.Id);

        var types = h.Db.ProductionEvents
            .OrderBy(e => e.Id)
            .Select(e => e.EventType)
            .ToList();

        Assert.Equal(new[]
        {
            ProductionEventType.JobReleased,
            ProductionEventType.JobStarted,
            ProductionEventType.JobPaused,
            ProductionEventType.JobResumed,
            ProductionEventType.JobCompleted
        }, types);

        var reloaded = h.Db.Jobs.Single();
        Assert.Equal(JobStatus.Completed, reloaded.Status);
        Assert.NotNull(reloaded.ReleasedUtc);
        Assert.NotNull(reloaded.StartedUtc);
        Assert.NotNull(reloaded.CompletedUtc);
    }

    [Fact]
    public async Task Release_is_idempotent_when_already_released()
    {
        using var h = new TestHarness();
        var job = NewJob();
        h.Db.Jobs.Add(job);
        await h.Db.SaveChangesAsync();

        await h.Jobs.ReleaseAsync(job.Id);
        await h.Jobs.ReleaseAsync(job.Id);

        Assert.Single(h.Db.ProductionEvents);
    }
}
