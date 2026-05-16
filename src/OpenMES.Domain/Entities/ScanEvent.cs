namespace OpenMES.Domain.Entities;

/// <summary>
/// Raw record of a scan/keyboard input from the floor. Kept even when parsing
/// fails so we can debug new barcode formats.
/// </summary>
public class ScanEvent
{
    public long Id { get; set; }
    public string RawValue { get; set; } = string.Empty;

    /// <summary>e.g. "JOB", "PART", "LOT", "EMP", or "Unknown".</summary>
    public string ParsedType { get; set; } = "Unknown";
    public string? ParsedKey { get; set; }
    public decimal? ParsedQuantity { get; set; }

    public int? JobId { get; set; }
    public int? ResourceId { get; set; }
    public int? UserId { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
