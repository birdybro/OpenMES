namespace OpenMES.Domain.Entities;

public class Part
{
    public int Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? UnitOfMeasure { get; set; }

    public List<PartRevision> Revisions { get; set; } = new();
}
