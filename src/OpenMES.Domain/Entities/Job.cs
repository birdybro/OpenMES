using OpenMES.Domain.Enums;

namespace OpenMES.Domain.Entities;

public class Job
{
    public int Id { get; set; }

    /// <summary>External-facing job number (often comes from the ERP).</summary>
    public string JobNumber { get; set; } = string.Empty;

    public int PartRevisionId { get; set; }
    public PartRevision? PartRevision { get; set; }

    /// <summary>Optional default resource — actual run resource is captured in events.</summary>
    public int? ResourceId { get; set; }
    public Resource? Resource { get; set; }

    public decimal QuantityOrdered { get; set; }
    public decimal QuantityGood { get; set; }
    public decimal QuantityScrap { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Created;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReleasedUtc { get; set; }
    public DateTime? StartedUtc { get; set; }
    public DateTime? CompletedUtc { get; set; }
    public DateTime? DueUtc { get; set; }

    public string? Notes { get; set; }
}
