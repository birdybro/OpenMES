using OpenMES.Domain.Enums;

namespace OpenMES.Domain.Entities;

/// <summary>
/// Append-only record of something that happened on the shop floor.
/// Never updated in place — corrections are new events.
/// </summary>
public class ProductionEvent
{
    public long Id { get; set; }
    public ProductionEventType EventType { get; set; }

    public int? JobId { get; set; }
    public Job? Job { get; set; }

    public int? ResourceId { get; set; }
    public Resource? Resource { get; set; }

    public int? OperationId { get; set; }
    public Operation? Operation { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    public decimal? Quantity { get; set; }
    public string? ReasonCode { get; set; }
    public string? Notes { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Original raw scan, if this event was triggered by a barcode.</summary>
    public string? RawScanValue { get; set; }
}
