using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Barcodes;

/// <summary>
/// Recognises a small, deliberately boring set of prefixed scans:
/// <c>JOB:10001</c>, <c>PART:ABC-123</c>, <c>LOT:RESIN-BLACK|QTY:500</c>,
/// <c>EMP:12345</c>, <c>RES:CNC-01</c>, <c>OP:OP010</c>.
/// </summary>
public sealed class SimpleBarcodeParser : IBarcodeParser
{
    public ParsedBarcode? TryParse(string rawScan)
    {
        if (string.IsNullOrWhiteSpace(rawScan))
        {
            return null;
        }

        var trimmed = rawScan.Trim();
        var colon = trimmed.IndexOf(':');
        if (colon <= 0 || colon == trimmed.Length - 1)
        {
            return null;
        }

        var prefix = trimmed[..colon].ToUpperInvariant();
        var rest = trimmed[(colon + 1)..];

        return prefix switch
        {
            "JOB" => Make(BarcodeKind.Job, rest, trimmed),
            "PART" => Make(BarcodeKind.Part, rest, trimmed),
            "EMP" => Make(BarcodeKind.Employee, rest, trimmed),
            "RES" => Make(BarcodeKind.Resource, rest, trimmed),
            "OP" => Make(BarcodeKind.Operation, rest, trimmed),
            "LOT" => ParseLot(rest, trimmed),
            _ => null
        };
    }

    private static ParsedBarcode Make(BarcodeKind kind, string key, string raw)
        => new(kind, key.Trim(), null, raw);

    private static ParsedBarcode? ParseLot(string rest, string raw)
    {
        // Accepts "RESIN-BLACK" or "RESIN-BLACK|QTY:500"
        var pipe = rest.IndexOf('|');
        if (pipe < 0)
        {
            var lot = rest.Trim();
            return lot.Length == 0 ? null : new ParsedBarcode(BarcodeKind.MaterialLot, lot, null, raw);
        }

        var lotKey = rest[..pipe].Trim();
        var tail = rest[(pipe + 1)..].Trim();
        if (lotKey.Length == 0)
        {
            return null;
        }

        // tail is expected to be "QTY:<number>"
        var tailColon = tail.IndexOf(':');
        decimal? qty = null;
        if (tailColon > 0
            && string.Equals(tail[..tailColon], "QTY", StringComparison.OrdinalIgnoreCase)
            && decimal.TryParse(tail[(tailColon + 1)..], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var parsed))
        {
            qty = parsed;
        }

        return new ParsedBarcode(BarcodeKind.MaterialLot, lotKey, qty, raw);
    }
}
