using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenMES.Domain;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;
using OpenMES.Infrastructure.Persistence;

namespace OpenMES.Infrastructure.Seeding;

/// <summary>
/// Inserts a demo dataset so the operator UI has content from first launch:
/// 3 resources, 5 parts (with revisions + operations), several jobs, sample
/// documents, material lots, and a handful of production events.
/// </summary>
public sealed class DataSeeder
{
    private readonly OpenMesDbContext _db;
    private readonly ILogger<DataSeeder> _log;

    public DataSeeder(OpenMesDbContext db, ILogger<DataSeeder> log)
    {
        _db = db;
        _log = log;
    }

    public async Task SeedIfEmptyAsync(CancellationToken ct = default)
    {
        if (await _db.Parts.AnyAsync(ct))
        {
            _log.LogDebug("Database already has parts — skipping seed.");
            return;
        }
        await SeedAsync(ct);
    }

    public async Task ReseedAsync(CancellationToken ct = default)
    {
        _log.LogInformation("Wiping demo data and re-seeding.");
        _db.RemoveRange(_db.ProductionEvents);
        _db.RemoveRange(_db.ScanEvents);
        _db.RemoveRange(_db.JobMaterialIssues);
        _db.RemoveRange(_db.QualityResults);
        _db.RemoveRange(_db.QualityChecks);
        _db.RemoveRange(_db.ResourceSchedule);
        _db.RemoveRange(_db.Jobs);
        _db.RemoveRange(_db.DocumentLinks);
        _db.RemoveRange(_db.Documents);
        _db.RemoveRange(_db.MaterialLots);
        _db.RemoveRange(_db.Operations);
        _db.RemoveRange(_db.PartRevisions);
        _db.RemoveRange(_db.Parts);
        _db.RemoveRange(_db.Resources);
        _db.RemoveRange(_db.Users);
        await _db.SaveChangesAsync(ct);
        await SeedAsync(ct);
    }

    private async Task SeedAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        // Users
        var users = new[]
        {
            new User { Code = "OP-001", DisplayName = "Alex Carter",   Role = Roles.Operator },
            new User { Code = "OP-002", DisplayName = "Priya Patel",   Role = Roles.Operator },
            new User { Code = "TE-001", DisplayName = "Sam Nguyen",    Role = Roles.Technician },
            new User { Code = "QA-001", DisplayName = "Jordan Reeves", Role = Roles.Quality },
            new User { Code = "SU-001", DisplayName = "Morgan Lopez",  Role = Roles.Supervisor },
            new User { Code = "AD-001", DisplayName = "Admin",         Role = Roles.Admin }
        };
        _db.Users.AddRange(users);

        // Resources / work centres
        var cnc01 = new Resource { Code = "CNC-01", Name = "Haas VF-2 (Cell A)", ResourceType = ResourceType.Machine, Location = "Bay 1" };
        var cnc02 = new Resource { Code = "CNC-02", Name = "Haas VF-4 (Cell A)", ResourceType = ResourceType.Machine, Location = "Bay 1" };
        var asm01 = new Resource { Code = "ASM-01", Name = "Assembly Bench", ResourceType = ResourceType.WorkCenter, Location = "Bay 3" };
        _db.Resources.AddRange(cnc01, cnc02, asm01);

        // Parts + revisions + operations
        var (p1, p1r) = MakePart("BRK-100", "Mounting bracket", "B");
        var (p2, p2r) = MakePart("SHF-220", "Drive shaft", "C");
        var (p3, p3r) = MakePart("PLT-310", "Top plate", "A");
        var (p4, p4r) = MakePart("GSK-405", "Gasket, neoprene", "A");
        var (p5, p5r) = MakePart("ASM-900", "Final assembly", "D");

        _db.Parts.AddRange(p1, p2, p3, p4, p5);

        AddOps(p1r, ("OP010", "Saw billet to length", "CNC-01"),
                    ("OP020", "Mill faces + drill mounting holes", "CNC-01"),
                    ("OP030", "Deburr + inspect", "ASM-01"));
        AddOps(p2r, ("OP010", "Turn OD on lathe", "CNC-02"),
                    ("OP020", "Mill keyway", "CNC-01"));
        AddOps(p3r, ("OP010", "Mill profile", "CNC-01"),
                    ("OP020", "Drill + tap hole pattern", "CNC-01"));
        AddOps(p4r, ("OP010", "Die-cut gasket", "ASM-01"));
        AddOps(p5r, ("OP010", "Sub-assemble bracket + shaft", "ASM-01"),
                    ("OP020", "Final assembly + test", "ASM-01"));

        // Documents
        var docs = new[]
        {
            new Document { Title = "BRK-100 work instructions (Rev B)", DocumentType = DocumentType.WorkInstruction, PartNumber = "BRK-100", Revision = "B", IsReleased = true, EffectiveDate = now.AddMonths(-2), UrlOrPath = "samples/docs/BRK-100-RevB-WI.pdf" },
            new Document { Title = "BRK-100 OP020 setup sheet",         DocumentType = DocumentType.SetupSheet,      PartNumber = "BRK-100", Revision = "B", OperationCode = "OP020", IsReleased = true, EffectiveDate = now.AddMonths(-2), UrlOrPath = "samples/docs/BRK-100-OP020-setup.pdf" },
            new Document { Title = "BRK-100 drawing (Rev B)",            DocumentType = DocumentType.Drawing,         PartNumber = "BRK-100", Revision = "B", IsReleased = true, EffectiveDate = now.AddMonths(-2), UrlOrPath = "samples/docs/BRK-100-RevB.pdf" },
            new Document { Title = "BRK-100 drawing (Rev A, obsolete)", DocumentType = DocumentType.Drawing,         PartNumber = "BRK-100", Revision = "A", IsReleased = true, IsObsolete = true, UrlOrPath = "samples/docs/BRK-100-RevA.pdf" },
            new Document { Title = "SHF-220 work instructions (Rev C)", DocumentType = DocumentType.WorkInstruction, PartNumber = "SHF-220", Revision = "C", IsReleased = true, UrlOrPath = "samples/docs/SHF-220-RevC-WI.pdf" },
            new Document { Title = "PLT-310 work instructions",          DocumentType = DocumentType.WorkInstruction, PartNumber = "PLT-310", Revision = "A", IsReleased = true, UrlOrPath = "samples/docs/PLT-310-RevA-WI.pdf" },
            new Document { Title = "ASM-900 final assembly procedure",   DocumentType = DocumentType.Procedure,       PartNumber = "ASM-900", Revision = "D", IsReleased = true, UrlOrPath = "samples/docs/ASM-900-RevD-procedure.pdf" },
            new Document { Title = "CNC-01 safety / lockout procedure",  DocumentType = DocumentType.Safety,          ResourceCode = "CNC-01", IsReleased = true, UrlOrPath = "samples/docs/CNC-01-safety.pdf" }
        };
        _db.Documents.AddRange(docs);

        // Material lots
        var lots = new[]
        {
            new MaterialLot { LotCode = "RESIN-BLACK", PartNumber = "GSK-405", QuantityOnHand = 500, UnitOfMeasure = "EA", Supplier = "AcmePoly",   ReceivedUtc = now.AddDays(-30) },
            new MaterialLot { LotCode = "AL-6061-L023", PartNumber = "BRK-100", QuantityOnHand = 120, UnitOfMeasure = "EA", Supplier = "MetalsCo",  ReceivedUtc = now.AddDays(-12) },
            new MaterialLot { LotCode = "STEEL-1018-22", PartNumber = "SHF-220", QuantityOnHand = 80,  UnitOfMeasure = "EA", Supplier = "MetalsCo",  ReceivedUtc = now.AddDays(-8) },
            new MaterialLot { LotCode = "STEEL-1018-19", PartNumber = "SHF-220", QuantityOnHand = 0,   UnitOfMeasure = "EA", Status = MaterialLotStatus.Consumed, Supplier = "MetalsCo", ReceivedUtc = now.AddDays(-60) },
            new MaterialLot { LotCode = "AL-6061-HOLD",  PartNumber = "PLT-310", QuantityOnHand = 60,  UnitOfMeasure = "EA", Status = MaterialLotStatus.OnHold, Notes = "Awaiting incoming inspection.", Supplier = "MetalsCo", ReceivedUtc = now.AddDays(-2) }
        };
        _db.MaterialLots.AddRange(lots);

        // Jobs
        var jobs = new[]
        {
            new Job { JobNumber = "10001", PartRevision = p1r, Resource = cnc01, QuantityOrdered = 50, Status = JobStatus.InProgress, ReleasedUtc = now.AddDays(-2), StartedUtc = now.AddHours(-6), DueUtc = now.AddDays(2),  QuantityGood = 18, QuantityScrap = 1 },
            new Job { JobNumber = "10002", PartRevision = p2r, Resource = cnc02, QuantityOrdered = 25, Status = JobStatus.Released,   ReleasedUtc = now.AddDays(-1),                              DueUtc = now.AddDays(3) },
            new Job { JobNumber = "10003", PartRevision = p3r, Resource = cnc01, QuantityOrdered = 40, Status = JobStatus.Created,                                                                  DueUtc = now.AddDays(5) },
            new Job { JobNumber = "10004", PartRevision = p4r, Resource = asm01, QuantityOrdered = 200, Status = JobStatus.Released,  ReleasedUtc = now.AddDays(-1),                              DueUtc = now.AddDays(1) },
            new Job { JobNumber = "10005", PartRevision = p5r, Resource = asm01, QuantityOrdered = 20, Status = JobStatus.Paused,     ReleasedUtc = now.AddDays(-3), StartedUtc = now.AddDays(-2), DueUtc = now.AddDays(2),  QuantityGood = 8, Notes = "Waiting on sub-component." },
            new Job { JobNumber = "10006", PartRevision = p1r, Resource = cnc01, QuantityOrdered = 30, Status = JobStatus.Completed,  ReleasedUtc = now.AddDays(-10), StartedUtc = now.AddDays(-9), CompletedUtc = now.AddDays(-7), DueUtc = now.AddDays(-6), QuantityGood = 30 }
        };
        _db.Jobs.AddRange(jobs);

        // Schedule (forward-looking)
        _db.ResourceSchedule.AddRange(
            new ResourceScheduleEntry { Resource = cnc01, Job = jobs[0], PlannedStartUtc = now.AddHours(-6), PlannedEndUtc = now.AddHours(6), PlannedQuantity = 50 },
            new ResourceScheduleEntry { Resource = cnc01, Job = jobs[2], PlannedStartUtc = now.AddHours(8), PlannedEndUtc = now.AddHours(24), PlannedQuantity = 40 },
            new ResourceScheduleEntry { Resource = cnc02, Job = jobs[1], PlannedStartUtc = now.AddHours(1), PlannedEndUtc = now.AddHours(10), PlannedQuantity = 25 },
            new ResourceScheduleEntry { Resource = asm01, Job = jobs[3], PlannedStartUtc = now, PlannedEndUtc = now.AddHours(6), PlannedQuantity = 200 },
            new ResourceScheduleEntry { Resource = asm01, Job = jobs[4], PlannedStartUtc = now.AddHours(8), PlannedEndUtc = now.AddHours(20), PlannedQuantity = 20 });

        // A small starter trail of production events on the in-progress job.
        var op = jobs[0];
        _db.ProductionEvents.AddRange(
            new ProductionEvent { EventType = ProductionEventType.JobCreated, Job = op, TimestampUtc = now.AddDays(-2) },
            new ProductionEvent { EventType = ProductionEventType.JobReleased, Job = op, TimestampUtc = now.AddDays(-2) },
            new ProductionEvent { EventType = ProductionEventType.MaterialIssued, Job = op, Quantity = 50, Notes = "Lot AL-6061-L023", TimestampUtc = now.AddHours(-7) },
            new ProductionEvent { EventType = ProductionEventType.JobStarted, Job = op, Resource = cnc01, TimestampUtc = now.AddHours(-6) },
            new ProductionEvent { EventType = ProductionEventType.GoodQuantityReported, Job = op, Resource = cnc01, Quantity = 18, TimestampUtc = now.AddMinutes(-90) },
            new ProductionEvent { EventType = ProductionEventType.ScrapQuantityReported, Job = op, Resource = cnc01, Quantity = 1, ReasonCode = "DIM-OOT", TimestampUtc = now.AddMinutes(-60) });

        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Seed complete: {Jobs} jobs, {Docs} documents, {Lots} lots.", jobs.Length, docs.Length, lots.Length);
    }

    private static (Part part, PartRevision rev) MakePart(string number, string desc, string revision)
    {
        var part = new Part { PartNumber = number, Description = desc, UnitOfMeasure = "EA" };
        var rev = new PartRevision { Part = part, Revision = revision, IsCurrent = true, ReleasedUtc = DateTime.UtcNow.AddMonths(-3) };
        part.Revisions.Add(rev);
        return (part, rev);
    }

    private static void AddOps(PartRevision rev, params (string Code, string Desc, string Resource)[] ops)
    {
        var seq = 10;
        foreach (var (code, desc, resource) in ops)
        {
            rev.Operations.Add(new Operation
            {
                OperationCode = code,
                Description = desc,
                PreferredResourceCode = resource,
                Sequence = seq,
                StandardSetupTimeMinutes = 15,
                StandardRunTimeMinutes = 2.5m
            });
            seq += 10;
        }
    }
}
