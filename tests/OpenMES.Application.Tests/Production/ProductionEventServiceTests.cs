using OpenMES.Application.Production;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;

namespace OpenMES.Application.Tests.Production;

public class ProductionEventServiceTests
{
    private static Job NewJob(int qty = 100) => new()
    {
        JobNumber = "J1",
        QuantityOrdered = qty,
        Status = JobStatus.Released
    };

    [Fact]
    public async Task RecordAsync_persists_event_with_clock_timestamp()
    {
        using var h = new TestHarness();
        var ts = h.Clock.UtcNow;

        await h.Events.RecordAsync(new ProductionEventInput(ProductionEventType.JobCreated));

        var events = h.Db.ProductionEvents.ToList();
        Assert.Single(events);
        Assert.Equal(ts, events[0].TimestampUtc);
        Assert.Equal(ProductionEventType.JobCreated, events[0].EventType);
    }

    [Fact]
    public async Task ReportGoodAsync_advances_job_quantity_and_starts_job()
    {
        using var h = new TestHarness();
        var job = NewJob();
        h.Db.Jobs.Add(job);
        await h.Db.SaveChangesAsync();

        await h.Events.ReportGoodAsync(job.Id, 5, userId: null);
        await h.Events.ReportGoodAsync(job.Id, 3, userId: null);

        var reloaded = h.Db.Jobs.Single();
        Assert.Equal(8, reloaded.QuantityGood);
        Assert.Equal(JobStatus.InProgress, reloaded.Status);
        Assert.NotNull(reloaded.StartedUtc);
    }

    [Fact]
    public async Task ReportScrapAsync_advances_scrap_only_and_records_reason()
    {
        using var h = new TestHarness();
        var job = NewJob();
        h.Db.Jobs.Add(job);
        await h.Db.SaveChangesAsync();

        await h.Events.ReportScrapAsync(job.Id, 2, "DIM-OOT", userId: null);

        var reloaded = h.Db.Jobs.Single();
        Assert.Equal(0, reloaded.QuantityGood);
        Assert.Equal(2, reloaded.QuantityScrap);

        var evt = h.Db.ProductionEvents.Single(e => e.EventType == ProductionEventType.ScrapQuantityReported);
        Assert.Equal("DIM-OOT", evt.ReasonCode);
    }

    [Fact]
    public async Task Events_are_append_only_and_ordered_newest_first_for_list()
    {
        using var h = new TestHarness();
        var job = NewJob();
        h.Db.Jobs.Add(job);
        await h.Db.SaveChangesAsync();

        await h.Events.RecordAsync(new ProductionEventInput(ProductionEventType.JobCreated, JobId: job.Id));
        h.Clock.Advance(TimeSpan.FromMinutes(1));
        await h.Events.RecordAsync(new ProductionEventInput(ProductionEventType.JobReleased, JobId: job.Id));
        h.Clock.Advance(TimeSpan.FromMinutes(1));
        await h.Events.RecordAsync(new ProductionEventInput(ProductionEventType.JobStarted, JobId: job.Id));

        var list = await h.Events.ListForJobAsync(job.Id);
        Assert.Equal(3, list.Count);
        Assert.Equal(ProductionEventType.JobStarted, list[0].EventType);
        Assert.Equal(ProductionEventType.JobReleased, list[1].EventType);
        Assert.Equal(ProductionEventType.JobCreated, list[2].EventType);
    }

    [Fact]
    public async Task Downtime_events_are_recorded_with_reason()
    {
        using var h = new TestHarness();
        var job = NewJob();
        h.Db.Jobs.Add(job);
        await h.Db.SaveChangesAsync();

        await h.Events.StartDowntimeAsync(job.Id, resourceId: 99, "TOOLCHANGE", userId: null);
        await h.Events.EndDowntimeAsync(job.Id, resourceId: 99, userId: null);

        var events = h.Db.ProductionEvents.OrderBy(e => e.Id).ToList();
        Assert.Equal(ProductionEventType.DowntimeStarted, events[0].EventType);
        Assert.Equal("TOOLCHANGE", events[0].ReasonCode);
        Assert.Equal(ProductionEventType.DowntimeEnded, events[1].EventType);
    }
}
