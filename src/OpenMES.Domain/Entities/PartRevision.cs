namespace OpenMES.Domain.Entities;

public class PartRevision
{
    public int Id { get; set; }
    public int PartId { get; set; }
    public Part? Part { get; set; }

    public string Revision { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public DateTime? ReleasedUtc { get; set; }
    public string? Notes { get; set; }

    public List<Operation> Operations { get; set; } = new();
}
