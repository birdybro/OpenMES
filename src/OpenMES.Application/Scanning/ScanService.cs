using OpenMES.Application.Abstractions;
using OpenMES.Application.Production;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Scanning;

public sealed record ScanProcessingResult(ScanEvent ScanEvent, ParsedBarcode? Parsed)
{
    public bool Recognised => Parsed is not null;
}

/// <summary>
/// Front door for any scanned input. Tries each registered
/// <see cref="IBarcodeParser"/> in registration order and persists a
/// <see cref="ScanEvent"/> regardless of success.
/// </summary>
public sealed class ScanService
{
    private readonly IEnumerable<IBarcodeParser> _parsers;
    private readonly IOpenMesDb _db;
    private readonly IClock _clock;
    private readonly ProductionEventService _events;

    public ScanService(
        IEnumerable<IBarcodeParser> parsers,
        IOpenMesDb db,
        IClock clock,
        ProductionEventService events)
    {
        _parsers = parsers;
        _db = db;
        _clock = clock;
        _events = events;
    }

    public ParsedBarcode? Parse(string rawScan)
    {
        foreach (var p in _parsers)
        {
            var hit = p.TryParse(rawScan);
            if (hit is not null) return hit;
        }
        return null;
    }

    public async Task<ScanProcessingResult> ProcessAsync(
        string rawScan,
        int? jobId = null,
        int? resourceId = null,
        int? userId = null,
        CancellationToken ct = default)
    {
        var parsed = Parse(rawScan);
        var scan = new ScanEvent
        {
            RawValue = rawScan,
            ParsedType = parsed?.Kind.ToString() ?? "Unknown",
            ParsedKey = parsed?.Key,
            ParsedQuantity = parsed?.Quantity,
            JobId = jobId,
            ResourceId = resourceId,
            UserId = userId,
            TimestampUtc = _clock.UtcNow
        };
        _db.Add(scan);
        await _db.SaveChangesAsync(ct);

        await _events.RecordAsync(new ProductionEventInput(
            ProductionEventType.BarcodeScanned,
            JobId: jobId,
            ResourceId: resourceId,
            UserId: userId,
            Notes: parsed is null ? "Unrecognised scan" : $"{parsed.Kind}:{parsed.Key}",
            RawScanValue: rawScan), ct);

        return new ScanProcessingResult(scan, parsed);
    }
}
