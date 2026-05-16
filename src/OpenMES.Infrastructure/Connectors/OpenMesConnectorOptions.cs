namespace OpenMES.Infrastructure.Connectors;

/// <summary>
/// Bound from the <c>OpenMes:Connectors</c> section of configuration. Any
/// connector whose path is null/empty is registered but yields nothing — the
/// sync still runs and reports zero fetched.
/// </summary>
public sealed class OpenMesConnectorOptions
{
    public const string SectionName = "OpenMes:Connectors";

    /// <summary>Path to a CSV file the <c>CsvJobConnector</c> will read.</summary>
    public string? CsvJobsPath { get; set; }

    /// <summary>Folder the <c>FileSystemDocumentConnector</c> will walk (top-level only).</summary>
    public string? FileSystemDocumentsRoot { get; set; }
}
