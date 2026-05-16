using Microsoft.Extensions.Logging.Abstractions;
using OpenMES.Infrastructure.Connectors;

namespace OpenMES.Application.Tests.Connectors;

public class CsvJobConnectorTests
{
    private static CsvJobConnector NewConnector(string? path)
        => new(new OpenMesConnectorOptions { CsvJobsPath = path }, NullLogger<CsvJobConnector>.Instance);

    private static string WriteTemp(string contents)
    {
        var p = Path.Combine(Path.GetTempPath(), $"openmes-csv-{Guid.NewGuid():N}.csv");
        File.WriteAllText(p, contents);
        return p;
    }

    [Fact]
    public async Task Returns_empty_when_path_not_configured()
    {
        var c = NewConnector(null);
        var list = new List<Domain.Entities.Job>();
        await foreach (var j in c.FetchJobsAsync()) list.Add(j);
        Assert.Empty(list);
    }

    [Fact]
    public async Task Returns_empty_when_file_missing()
    {
        var c = NewConnector("/tmp/definitely-not-here-" + Guid.NewGuid().ToString("N") + ".csv");
        var list = new List<Domain.Entities.Job>();
        await foreach (var j in c.FetchJobsAsync()) list.Add(j);
        Assert.Empty(list);
    }

    [Fact]
    public async Task Parses_basic_rows()
    {
        var path = WriteTemp("""
            job_number,part_number,revision,resource_code,quantity,due_utc,notes
            20001,BRK-100,B,CNC-01,25,2026-06-01T12:00:00Z,
            20002,SHF-220,C,,10,,Bare bones row
            """);
        try
        {
            var c = NewConnector(path);
            var list = new List<Domain.Entities.Job>();
            await foreach (var j in c.FetchJobsAsync()) list.Add(j);

            Assert.Equal(2, list.Count);
            Assert.Equal("20001", list[0].JobNumber);
            Assert.Equal(25, list[0].QuantityOrdered);
            Assert.Equal("BRK-100", list[0].PartRevision!.Part!.PartNumber);
            Assert.Equal("B", list[0].PartRevision!.Revision);
            Assert.Equal("CNC-01", list[0].Resource!.Code);
            Assert.Equal(new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc), list[0].DueUtc);

            Assert.Equal("20002", list[1].JobNumber);
            Assert.Null(list[1].Resource);
            Assert.Null(list[1].DueUtc);
            Assert.Equal("Bare bones row", list[1].Notes);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task Handles_quoted_field_with_comma()
    {
        var path = WriteTemp("""
            job_number,part_number,revision,resource_code,quantity,due_utc,notes
            20003,BRK-100,B,CNC-01,5,,"Rush order, expedite if possible"
            """);
        try
        {
            var c = NewConnector(path);
            var list = new List<Domain.Entities.Job>();
            await foreach (var j in c.FetchJobsAsync()) list.Add(j);

            Assert.Single(list);
            Assert.Equal("Rush order, expedite if possible", list[0].Notes);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task Skips_rows_with_invalid_quantity()
    {
        var path = WriteTemp("""
            job_number,part_number,revision,resource_code,quantity,due_utc,notes
            20004,BRK-100,B,CNC-01,not-a-number,,
            20005,BRK-100,B,CNC-01,7,,ok
            """);
        try
        {
            var c = NewConnector(path);
            var list = new List<Domain.Entities.Job>();
            await foreach (var j in c.FetchJobsAsync()) list.Add(j);

            Assert.Single(list);
            Assert.Equal("20005", list[0].JobNumber);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task Skips_rows_with_missing_required_field()
    {
        var path = WriteTemp("""
            job_number,part_number,revision,resource_code,quantity,due_utc,notes
            ,BRK-100,B,CNC-01,1,,
            20006,,B,CNC-01,1,,
            20007,BRK-100,,CNC-01,1,,
            20008,BRK-100,B,,3,,ok
            """);
        try
        {
            var c = NewConnector(path);
            var list = new List<Domain.Entities.Job>();
            await foreach (var j in c.FetchJobsAsync()) list.Add(j);

            Assert.Single(list);
            Assert.Equal("20008", list[0].JobNumber);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("a,b,c", new[] { "a", "b", "c" })]
    [InlineData("\"a,b\",c", new[] { "a,b", "c" })]
    [InlineData("\"a\"\"b\",c", new[] { "a\"b", "c" })]
    [InlineData("a,,c", new[] { "a", "", "c" })]
    [InlineData("", new string[0])]
    public void ParseCsvLine_handles_quoting_and_empties(string line, string[] expected)
    {
        var actual = CsvJobConnector.ParseCsvLine(line);
        Assert.Equal(expected, actual);
    }
}
