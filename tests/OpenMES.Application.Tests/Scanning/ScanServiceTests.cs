using OpenMES.Application.Barcodes;
using OpenMES.Domain.Enums;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Tests.Scanning;

public class ScanServiceTests
{
    [Fact]
    public async Task Unrecognised_scan_still_persists_a_ScanEvent_and_BarcodeScanned()
    {
        using var h = new TestHarness(parsers: new[] { new SimpleBarcodeParser() });
        var r = await h.Scans.ProcessAsync("totally unknown");
        Assert.False(r.Recognised);
        Assert.Single(h.Db.ScanEvents);
        Assert.Equal("Unknown", h.Db.ScanEvents.Single().ParsedType);
        Assert.Single(h.Db.ProductionEvents, e => e.EventType == ProductionEventType.BarcodeScanned);
    }

    [Fact]
    public async Task Recognised_lot_scan_extracts_kind_key_and_quantity()
    {
        using var h = new TestHarness(parsers: new[] { new SimpleBarcodeParser() });
        var r = await h.Scans.ProcessAsync("LOT:RESIN-BLACK|QTY:42");
        Assert.True(r.Recognised);
        Assert.Equal(BarcodeKind.MaterialLot, r.Parsed!.Kind);
        Assert.Equal("RESIN-BLACK", r.Parsed.Key);
        Assert.Equal(42m, r.Parsed.Quantity);

        var stored = h.Db.ScanEvents.Single();
        Assert.Equal("MaterialLot", stored.ParsedType);
        Assert.Equal("RESIN-BLACK", stored.ParsedKey);
        Assert.Equal(42m, stored.ParsedQuantity);
    }

    [Fact]
    public async Task First_matching_parser_wins()
    {
        using var h = new TestHarness(parsers: new IBarcodeParser[]
        {
            new StubParser(),
            new SimpleBarcodeParser()
        });
        var r = await h.Scans.ProcessAsync("LOT:RESIN-BLACK");
        Assert.True(r.Recognised);
        Assert.Equal("STUB", r.Parsed!.Key);
    }

    private sealed class StubParser : IBarcodeParser
    {
        public ParsedBarcode? TryParse(string raw)
            => new(BarcodeKind.Unknown, "STUB", null, raw);
    }
}
