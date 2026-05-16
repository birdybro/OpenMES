using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenMES.Application.Abstractions;
using OpenMES.Domain.Entities;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Sync;

/// <summary>
/// Pulls documents from every registered <see cref="IExternalDocumentConnector"/>
/// and upserts them. <see cref="Document.UrlOrPath"/> is treated as the
/// natural key — re-running the sync after a file changes updates the same
/// row in place.
/// </summary>
public sealed class DocumentSyncService
{
    private readonly IOpenMesDb _db;
    private readonly IClock _clock;
    private readonly IEnumerable<IExternalDocumentConnector> _connectors;
    private readonly ILogger<DocumentSyncService> _log;

    public DocumentSyncService(
        IOpenMesDb db,
        IClock clock,
        IEnumerable<IExternalDocumentConnector> connectors,
        ILogger<DocumentSyncService> log)
    {
        _db = db;
        _clock = clock;
        _connectors = connectors;
        _log = log;
    }

    public async Task<SyncReport> SyncAsync(DateTime? changedSinceUtc = null, CancellationToken ct = default)
    {
        var report = new SyncReport();

        foreach (var connector in _connectors)
        {
            await foreach (var staged in connector.FetchDocumentsAsync(changedSinceUtc, ct).WithCancellation(ct))
            {
                report.Fetched++;
                try
                {
                    await ApplyAsync(staged, report, ct);
                }
                catch (Exception ex)
                {
                    report.Errors++;
                    report.ErrorMessages.Add($"Document '{staged.Title}' ({staged.UrlOrPath}): {ex.Message}");
                    _log.LogError(ex, "Document sync failed for {Path}.", staged.UrlOrPath);
                }
            }
        }

        report.FinishedUtc = _clock.UtcNow;
        return report;
    }

    private async Task ApplyAsync(Document staged, SyncReport report, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(staged.UrlOrPath))
        {
            report.Skipped++;
            report.SkipReasons.Add($"Document '{staged.Title}' has no path — skipped.");
            return;
        }

        var existing = await _db.Documents.FirstOrDefaultAsync(d => d.UrlOrPath == staged.UrlOrPath, ct);
        if (existing is null)
        {
            _db.Add(staged);
            await _db.SaveChangesAsync(ct);
            report.Inserted++;
        }
        else
        {
            existing.Title = staged.Title;
            existing.DocumentType = staged.DocumentType;
            existing.PartNumber = staged.PartNumber;
            existing.Revision = staged.Revision;
            existing.OperationCode = staged.OperationCode;
            existing.ResourceCode = staged.ResourceCode;
            existing.EffectiveDate = staged.EffectiveDate;
            existing.IsReleased = staged.IsReleased;
            existing.IsObsolete = staged.IsObsolete;
            await _db.SaveChangesAsync(ct);
            report.Updated++;
        }
    }
}
