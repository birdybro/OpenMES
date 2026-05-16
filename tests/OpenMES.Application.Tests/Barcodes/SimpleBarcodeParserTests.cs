using OpenMES.Application.Barcodes;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Tests.Barcodes;

public class SimpleBarcodeParserTests
{
    private readonly SimpleBarcodeParser _parser = new();

    [Theory]
    [InlineData("JOB:10001", BarcodeKind.Job, "10001")]
    [InlineData("PART:ABC-123", BarcodeKind.Part, "ABC-123")]
    [InlineData("EMP:12345", BarcodeKind.Employee, "12345")]
    [InlineData("RES:CNC-01", BarcodeKind.Resource, "CNC-01")]
    [InlineData("OP:OP010", BarcodeKind.Operation, "OP010")]
    [InlineData("LOT:RESIN-BLACK", BarcodeKind.MaterialLot, "RESIN-BLACK")]
    public void Parses_simple_prefixes(string raw, BarcodeKind kind, string key)
    {
        var p = _parser.TryParse(raw);
        Assert.NotNull(p);
        Assert.Equal(kind, p!.Kind);
        Assert.Equal(key, p.Key);
        Assert.Null(p.Quantity);
    }

    [Fact]
    public void Lot_with_quantity_parses_qty()
    {
        var p = _parser.TryParse("LOT:RESIN-BLACK|QTY:500");
        Assert.NotNull(p);
        Assert.Equal(BarcodeKind.MaterialLot, p!.Kind);
        Assert.Equal("RESIN-BLACK", p.Key);
        Assert.Equal(500m, p.Quantity);
    }

    [Fact]
    public void Lot_with_decimal_quantity_parses_qty()
    {
        var p = _parser.TryParse("LOT:RESIN-BLACK|QTY:12.5");
        Assert.NotNull(p);
        Assert.Equal(12.5m, p!.Quantity);
    }

    [Fact]
    public void Lot_without_qty_after_pipe_still_parses_lot()
    {
        var p = _parser.TryParse("LOT:RESIN-BLACK|");
        Assert.NotNull(p);
        Assert.Equal("RESIN-BLACK", p!.Key);
        Assert.Null(p.Quantity);
    }

    [Fact]
    public void Case_insensitive_on_prefix()
    {
        var p = _parser.TryParse("job:10001");
        Assert.NotNull(p);
        Assert.Equal(BarcodeKind.Job, p!.Kind);
    }

    [Fact]
    public void Trims_whitespace()
    {
        var p = _parser.TryParse("  PART:ABC-123  ");
        Assert.NotNull(p);
        Assert.Equal("ABC-123", p!.Key);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no-prefix")]
    [InlineData(":missing-prefix")]
    [InlineData("PREFIX:")]
    [InlineData("XYZ:something")]
    public void Returns_null_for_unrecognised_input(string raw)
    {
        Assert.Null(_parser.TryParse(raw));
    }

    [Fact]
    public void Raw_value_is_preserved()
    {
        var p = _parser.TryParse(" LOT:X|QTY:1 ")!;
        Assert.Equal("LOT:X|QTY:1", p.Raw);
    }
}
