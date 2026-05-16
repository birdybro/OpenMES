using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;
using OpenMES.PluginAbstractions;

namespace OpenMES.Infrastructure.Connectors;

/// <summary>
/// Reference <see cref="IExternalDocumentConnector"/> that walks a folder
/// (top-level, non-recursive) and emits <see cref="Document"/> records based
/// on filename conventions.
///
/// Filename grammar (underscore separated, case insensitive on the type
/// suffix; extension is preserved as-is):
///
/// <list type="bullet">
/// <item><c>{part}_{rev}.{ext}</c> — drawing on (part, rev)</item>
/// <item><c>{part}_{rev}_wi.{ext}</c> — work instruction</item>
/// <item><c>{part}_{rev}_procedure.{ext}</c> — procedure</item>
/// <item><c>{part}_{rev}_quality.{ext}</c> — quality document</item>
/// <item><c>{part}_{rev}_op_{opcode}_setup.{ext}</c> — setup sheet for an op</item>
/// <item><c>{part}_{rev}_op_{opcode}_{typeKey}.{ext}</c> — typed doc on an op</item>
/// <item><c>resource_{code}_{typeKey}.{ext}</c> — resource-scoped doc</item>
/// </list>
///
/// Files that don't match are skipped (with a debug log) — operators can
/// drop arbitrary supporting files in the folder without polluting the
/// document catalogue.
/// </summary>
public sealed class FileSystemDocumentConnector : IExternalDocumentConnector
{
    private readonly OpenMesConnectorOptions _options;
    private readonly ILogger<FileSystemDocumentConnector> _log;

    public FileSystemDocumentConnector(IOptions<OpenMesConnectorOptions> options, ILogger<FileSystemDocumentConnector> log)
        : this(options.Value, log) { }

    public FileSystemDocumentConnector(OpenMesConnectorOptions options, ILogger<FileSystemDocumentConnector> log)
    {
        _options = options;
        _log = log;
    }

    public async IAsyncEnumerable<Document> FetchDocumentsAsync(
        DateTime? changedSinceUtc = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var root = _options.FileSystemDocumentsRoot;
        if (string.IsNullOrWhiteSpace(root))
        {
            _log.LogDebug("FileSystemDocumentConnector has no root configured — nothing to fetch.");
            yield break;
        }
        if (!Directory.Exists(root))
        {
            _log.LogWarning("FileSystemDocumentConnector root '{Root}' does not exist — nothing to fetch.", root);
            yield break;
        }

        foreach (var file in Directory.EnumerateFiles(root))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fi = new FileInfo(file);
            if (changedSinceUtc is DateTime since && fi.LastWriteTimeUtc < since)
            {
                continue;
            }
            var doc = TryParse(fi);
            if (doc is null)
            {
                _log.LogDebug("Skipping unrecognised filename '{Name}'.", fi.Name);
                continue;
            }
            yield return doc;
            await Task.Yield();
        }
    }

    internal static Document? TryParse(FileInfo fi)
    {
        var stem = Path.GetFileNameWithoutExtension(fi.Name);
        if (stem.Length == 0) return null;

        var parts = stem.Split('_');
        if (parts.Length < 2) return null;

        // Resource-scoped: resource_{code}_{typeKey}
        if (string.Equals(parts[0], "resource", StringComparison.OrdinalIgnoreCase))
        {
            if (parts.Length < 3) return null;
            var resourceCode = parts[1];
            var typeKey = string.Join('_', parts.Skip(2));
            var type = MapTypeKey(typeKey, DocumentType.Other);
            return new Document
            {
                Title = Humanise(stem),
                DocumentType = type,
                ResourceCode = resourceCode,
                IsReleased = true,
                EffectiveDate = fi.LastWriteTimeUtc,
                UrlOrPath = fi.FullName
            };
        }

        // Otherwise: {part}_{rev}[ _op_{opcode}_{typeKey} | _{typeKey} ]
        var partNumber = parts[0];
        var revision = parts[1];

        string? opCode = null;
        string? typeKeyRest = null;

        if (parts.Length >= 3)
        {
            if (string.Equals(parts[2], "op", StringComparison.OrdinalIgnoreCase))
            {
                if (parts.Length < 5) return null;          // op requires opcode + type
                opCode = parts[3];
                typeKeyRest = string.Join('_', parts.Skip(4));
            }
            else
            {
                typeKeyRest = string.Join('_', parts.Skip(2));
            }
        }

        var docType = typeKeyRest is null
            ? DocumentType.Drawing                          // {part}_{rev} alone
            : MapTypeKey(typeKeyRest, DocumentType.Other);

        return new Document
        {
            Title = Humanise(stem),
            DocumentType = docType,
            PartNumber = partNumber,
            Revision = revision,
            OperationCode = opCode,
            IsReleased = true,
            EffectiveDate = fi.LastWriteTimeUtc,
            UrlOrPath = fi.FullName
        };
    }

    private static DocumentType MapTypeKey(string typeKey, DocumentType fallback)
        => typeKey.ToLowerInvariant() switch
        {
            "wi" or "instructions" or "work_instructions" => DocumentType.WorkInstruction,
            "setup" or "setup_sheet" => DocumentType.SetupSheet,
            "drawing" or "dwg" => DocumentType.Drawing,
            "quality" or "qa" => DocumentType.QualityDocument,
            "procedure" or "proc" => DocumentType.Procedure,
            "safety" or "lockout" => DocumentType.Safety,
            _ => fallback
        };

    private static string Humanise(string stem)
        => stem.Replace('_', ' ');
}
