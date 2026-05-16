using OpenMES.Domain.Enums;

namespace OpenMES.Domain.Entities;

public class QualityCheck
{
    public int Id { get; set; }

    public int OperationId { get; set; }
    public Operation? Operation { get; set; }

    public string Title { get; set; } = string.Empty;
    public QualityCheckType CheckType { get; set; }

    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public string? Unit { get; set; }

    public bool Required { get; set; } = true;
    public string? Instructions { get; set; }
}
