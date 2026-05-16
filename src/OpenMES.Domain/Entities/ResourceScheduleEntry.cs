namespace OpenMES.Domain.Entities;

public class ResourceScheduleEntry
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public Resource? Resource { get; set; }

    public int JobId { get; set; }
    public Job? Job { get; set; }

    public DateTime PlannedStartUtc { get; set; }
    public DateTime PlannedEndUtc { get; set; }
    public decimal? PlannedQuantity { get; set; }
    public string? Notes { get; set; }
}
