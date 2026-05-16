namespace OpenMES.Domain.Entities;

public class JobMaterialIssue
{
    public int Id { get; set; }

    public int JobId { get; set; }
    public Job? Job { get; set; }

    public int MaterialLotId { get; set; }
    public MaterialLot? MaterialLot { get; set; }

    public decimal Quantity { get; set; }
    public DateTime IssuedUtc { get; set; } = DateTime.UtcNow;
    public int? IssuedByUserId { get; set; }
    public string? Notes { get; set; }
}
