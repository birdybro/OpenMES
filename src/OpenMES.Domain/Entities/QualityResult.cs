namespace OpenMES.Domain.Entities;

public class QualityResult
{
    public int Id { get; set; }

    public int QualityCheckId { get; set; }
    public QualityCheck? QualityCheck { get; set; }

    public int JobId { get; set; }
    public Job? Job { get; set; }

    public decimal? NumericValue { get; set; }
    public string? TextValue { get; set; }
    public bool Pass { get; set; }

    public DateTime RecordedUtc { get; set; } = DateTime.UtcNow;
    public int? RecordedByUserId { get; set; }
    public string? Notes { get; set; }
}
