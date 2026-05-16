namespace OpenMES.Domain.Entities;

public class Operation
{
    public int Id { get; set; }
    public int PartRevisionId { get; set; }
    public PartRevision? PartRevision { get; set; }

    public string OperationCode { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Preferred resource code (work center) for this op. Optional —
    /// scheduling may override.
    /// </summary>
    public string? PreferredResourceCode { get; set; }

    public decimal? StandardRunTimeMinutes { get; set; }
    public decimal? StandardSetupTimeMinutes { get; set; }
}
