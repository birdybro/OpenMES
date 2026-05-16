using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMES.Domain.Entities;
using OpenMES.PluginAbstractions;

namespace OpenMES.Infrastructure.Connectors;

/// <summary>
/// Reference <see cref="IExternalJobConnector"/> that reads jobs from a CSV
/// file. Schema (header row required):
/// <c>job_number,part_number,revision,resource_code,quantity,due_utc,notes</c>.
/// <c>resource_code</c>, <c>due_utc</c>, and <c>notes</c> are optional.
/// Quoted fields (<c>"like, this"</c>) are supported; embedded <c>""</c>
/// escapes a literal quote.
/// </summary>
public sealed class CsvJobConnector : IExternalJobConnector
{
    private readonly OpenMesConnectorOptions _options;
    private readonly ILogger<CsvJobConnector> _log;

    public CsvJobConnector(IOptions<OpenMesConnectorOptions> options, ILogger<CsvJobConnector> log)
        : this(options.Value, log) { }

    // Test-friendly constructor.
    public CsvJobConnector(OpenMesConnectorOptions options, ILogger<CsvJobConnector> log)
    {
        _options = options;
        _log = log;
    }

    public async IAsyncEnumerable<Job> FetchJobsAsync(
        DateTime? changedSinceUtc = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var path = _options.CsvJobsPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            _log.LogDebug("CsvJobConnector has no path configured — nothing to fetch.");
            yield break;
        }
        if (!File.Exists(path))
        {
            _log.LogWarning("CsvJobConnector path '{Path}' does not exist — nothing to fetch.", path);
            yield break;
        }

        using var reader = new StreamReader(path);
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (headerLine is null)
        {
            yield break;
        }
        var header = ParseCsvLine(headerLine)
            .Select((h, i) => (h.Trim(), i))
            .ToDictionary(t => t.Item1, t => t.i, StringComparer.OrdinalIgnoreCase);

        if (!header.ContainsKey("job_number") || !header.ContainsKey("part_number") || !header.ContainsKey("revision") || !header.ContainsKey("quantity"))
        {
            _log.LogWarning("CSV at '{Path}' is missing one of the required headers (job_number, part_number, revision, quantity).", path);
            yield break;
        }

        int lineNo = 1;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var raw = await reader.ReadLineAsync(cancellationToken);
            if (raw is null) break;
            lineNo++;
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var fields = ParseCsvLine(raw);
            string Get(string col) => header.TryGetValue(col, out var i) && i < fields.Count ? fields[i] : string.Empty;

            var jobNumber = Get("job_number").Trim();
            var partNumber = Get("part_number").Trim();
            var revision = Get("revision").Trim();
            var qtyText = Get("quantity").Trim();
            if (jobNumber.Length == 0 || partNumber.Length == 0 || revision.Length == 0)
            {
                _log.LogWarning("Skipping CSV line {Line}: required field empty.", lineNo);
                continue;
            }
            if (!decimal.TryParse(qtyText, NumberStyles.Any, CultureInfo.InvariantCulture, out var qty))
            {
                _log.LogWarning("Skipping CSV line {Line}: invalid quantity '{Qty}'.", lineNo, qtyText);
                continue;
            }

            var resourceCode = Get("resource_code").Trim();
            var dueText = Get("due_utc").Trim();
            DateTime? due = null;
            if (dueText.Length > 0
                && DateTime.TryParse(dueText, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var d))
            {
                due = DateTime.SpecifyKind(d, DateTimeKind.Utc);
            }

            var notes = Get("notes");
            if (string.IsNullOrWhiteSpace(notes)) notes = null;

            yield return new Job
            {
                JobNumber = jobNumber,
                QuantityOrdered = qty,
                DueUtc = due,
                Notes = notes,
                PartRevision = new PartRevision
                {
                    Revision = revision,
                    Part = new Part { PartNumber = partNumber }
                },
                Resource = string.IsNullOrEmpty(resourceCode)
                    ? null
                    : new Resource { Code = resourceCode }
            };
        }
    }

    /// <summary>
    /// Minimal RFC-4180-ish CSV line parser. Handles quoted fields with
    /// embedded commas and <c>""</c>-escaped quotes. Does not support
    /// newline-bearing fields — keep CSVs single-line per row.
    /// </summary>
    internal static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        if (line.Length == 0) return fields;

        var sb = new System.Text.StringBuilder();
        var inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                if (c == ',')
                {
                    fields.Add(sb.ToString());
                    sb.Clear();
                }
                else if (c == '"' && sb.Length == 0)
                {
                    inQuotes = true;
                }
                else
                {
                    sb.Append(c);
                }
            }
        }
        fields.Add(sb.ToString());
        return fields;
    }
}
