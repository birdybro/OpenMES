namespace OpenMES.PluginAbstractions;

/// <summary>
/// Decodes a raw scan into a <see cref="ParsedBarcode"/>. Multiple
/// implementations may be registered; the first to return a non-null result
/// wins. Implementations should be cheap and safe to call concurrently.
/// </summary>
public interface IBarcodeParser
{
    /// <summary>
    /// Returns a parsed payload, or <c>null</c> if this parser does not
    /// recognise the scan.
    /// </summary>
    ParsedBarcode? TryParse(string rawScan);
}
