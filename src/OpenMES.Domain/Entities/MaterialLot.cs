using OpenMES.Domain.Enums;

namespace OpenMES.Domain.Entities;

public class MaterialLot
{
    public int Id { get; set; }
    public string LotCode { get; set; } = string.Empty;

    public string PartNumber { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = "EA";

    public decimal QuantityOnHand { get; set; }
    public MaterialLotStatus Status { get; set; } = MaterialLotStatus.Available;

    public DateTime? ReceivedUtc { get; set; }
    public DateTime? ExpiresUtc { get; set; }
    public string? Supplier { get; set; }
    public string? Notes { get; set; }
}
