using Microsoft.Extensions.Logging.Abstractions;
using OpenMES.Application.Sync;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Tests.Sync;

public class JobSyncServiceTests
{
    private static async Task<(Part part, PartRevision rev, Resource resource)> SeedMasters(TestHarness h)
    {
        var part = new Part { PartNumber = "P1" };
        var rev = new PartRevision { Part = part, Revision = "A", IsCurrent = true };
        var resource = new Resource { Code = "M1", Name = "Machine 1", ResourceType = ResourceType.Machine };
        h.Db.AddRange(part, rev, resource);
        await h.Db.SaveChangesAsync();
        return (part, rev, resource);
    }

    private static JobSyncService NewService(TestHarness h, params IExternalJobConnector[] connectors)
        => new(h.Data, h.Clock, connectors, h.Events, NullLogger<JobSyncService>.Instance);

    [Fact]
    public async Task Empty_connector_list_returns_empty_report()
    {
        using var h = new TestHarness();
        var svc = NewService(h);
        var report = await svc.SyncAsync();
        Assert.Equal(0, report.Fetched);
        Assert.NotNull(report.FinishedUtc);
    }

    [Fact]
    public async Task New_job_is_inserted_and_emits_JobCreated_event()
    {
        using var h = new TestHarness();
        await SeedMasters(h);

        var staged = new Job
        {
            JobNumber = "X1",
            QuantityOrdered = 10,
            DueUtc = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            PartRevision = new PartRevision { Revision = "A", Part = new Part { PartNumber = "P1" } },
            Resource = new Resource { Code = "M1" }
        };
        var svc = NewService(h, new StubConnector(staged));

        var report = await svc.SyncAsync();

        Assert.Equal(1, report.Fetched);
        Assert.Equal(1, report.Inserted);
        Assert.Equal(0, report.Updated);
        var saved = h.Db.Jobs.Single();
        Assert.Equal("X1", saved.JobNumber);
        Assert.NotNull(saved.ResourceId);
        Assert.Single(h.Db.ProductionEvents, e => e.EventType == ProductionEventType.JobCreated);
    }

    [Fact]
    public async Task Existing_job_is_updated_by_JobNumber_without_new_event()
    {
        using var h = new TestHarness();
        var (_, rev, resource) = await SeedMasters(h);
        h.Db.Jobs.Add(new Job
        {
            JobNumber = "X1",
            PartRevisionId = rev.Id,
            ResourceId = resource.Id,
            QuantityOrdered = 5,
            Status = JobStatus.InProgress,
            QuantityGood = 2
        });
        await h.Db.SaveChangesAsync();

        var staged = new Job
        {
            JobNumber = "X1",
            QuantityOrdered = 20,
            PartRevision = new PartRevision { Revision = "A", Part = new Part { PartNumber = "P1" } },
            Resource = new Resource { Code = "M1" }
        };
        var report = await NewService(h, new StubConnector(staged)).SyncAsync();

        Assert.Equal(0, report.Inserted);
        Assert.Equal(1, report.Updated);

        var saved = h.Db.Jobs.Single();
        Assert.Equal(20, saved.QuantityOrdered);    // synced
        Assert.Equal(2, saved.QuantityGood);        // floor-owned, untouched
        Assert.Equal(JobStatus.InProgress, saved.Status);
        Assert.DoesNotContain(h.Db.ProductionEvents, e => e.EventType == ProductionEventType.JobCreated);
    }

    [Fact]
    public async Task Missing_part_revision_is_skipped_with_reason()
    {
        using var h = new TestHarness();
        await SeedMasters(h);

        var staged = new Job
        {
            JobNumber = "X9",
            QuantityOrdered = 1,
            PartRevision = new PartRevision { Revision = "B", Part = new Part { PartNumber = "NOPE" } }
        };
        var report = await NewService(h, new StubConnector(staged)).SyncAsync();

        Assert.Equal(1, report.Fetched);
        Assert.Equal(1, report.Skipped);
        Assert.Empty(h.Db.Jobs);
        Assert.Contains("NOPE/B", report.SkipReasons.Single());
    }

    [Fact]
    public async Task Missing_resource_logs_reason_but_still_upserts_with_null_resource()
    {
        using var h = new TestHarness();
        await SeedMasters(h);

        var staged = new Job
        {
            JobNumber = "X1",
            QuantityOrdered = 3,
            PartRevision = new PartRevision { Revision = "A", Part = new Part { PartNumber = "P1" } },
            Resource = new Resource { Code = "GHOST" }
        };
        var report = await NewService(h, new StubConnector(staged)).SyncAsync();

        Assert.Equal(1, report.Inserted);
        Assert.Null(h.Db.Jobs.Single().ResourceId);
        Assert.Contains(report.SkipReasons, r => r.Contains("GHOST"));
    }

    [Fact]
    public async Task Fetched_counter_advances_even_when_skipped()
    {
        using var h = new TestHarness();
        await SeedMasters(h);

        var connector = new StubConnector(
            new Job { JobNumber = "X1", QuantityOrdered = 1, PartRevision = new PartRevision { Revision = "A", Part = new Part { PartNumber = "P1" } } },
            new Job { JobNumber = "X2", QuantityOrdered = 1, PartRevision = new PartRevision { Revision = "Z", Part = new Part { PartNumber = "MISS" } } });

        var report = await NewService(h, connector).SyncAsync();
        Assert.Equal(2, report.Fetched);
        Assert.Equal(1, report.Inserted);
        Assert.Equal(1, report.Skipped);
    }

    private sealed class StubConnector : IExternalJobConnector
    {
        private readonly Job[] _jobs;
        public StubConnector(params Job[] jobs) => _jobs = jobs;
        public async IAsyncEnumerable<Job> FetchJobsAsync(
            DateTime? changedSinceUtc = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var j in _jobs)
            {
                ct.ThrowIfCancellationRequested();
                yield return j;
                await Task.Yield();
            }
        }
    }
}
