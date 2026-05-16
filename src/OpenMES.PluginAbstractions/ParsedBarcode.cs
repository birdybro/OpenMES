namespace OpenMES.PluginAbstractions;

/// <summary>
/// Standard barcode payload categories the rest of the system reasons about.
/// </summary>
public enum BarcodeKind
{
    Unknown = 0,
    Job = 1,
    Part = 2,
    MaterialLot = 3,
    Employee = 4,
    Resource = 5,
    Operation = 6
}

/// <summary>
/// A parsed scan. <see cref="Kind"/> is the category; <see cref="Key"/> is the
/// primary identifier (job number, part number, lot code, employee code, …);
/// <see cref="Quantity"/> is set when the scan carries one (e.g. LOT + QTY).
/// </summary>
public sealed record ParsedBarcode(
    BarcodeKind Kind,
    string Key,
    decimal? Quantity = null,
    string? Raw = null,
    IReadOnlyDictionary<string, string>? Extras = null);
