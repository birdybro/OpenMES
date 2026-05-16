using Microsoft.EntityFrameworkCore;
using OpenMES.Application.Abstractions;
using OpenMES.Domain.Entities;
using OpenMES.Domain.Enums;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application.Documents;

/// <summary>
/// Built-in resolver. Returns released, non-obsolete documents that match the
/// job's part / revision / operation / resource via either the inline scope
/// fields on <see cref="Document"/> or explicit <see cref="DocumentLink"/>
/// rows. Results are ranked: operation > revision > part > resource.
/// </summary>
public sealed class DefaultDocumentResolver : IDocumentResolver
{
    private readonly IOpenMesDb _db;

    public DefaultDocumentResolver(IOpenMesDb db) => _db = db;

    public async Task<IReadOnlyList<Document>> ResolveAsync(
        DocumentResolutionContext context,
        CancellationToken cancellationToken = default)
    {
        var part = context.PartNumber;
        var rev = context.Revision;
        var op = context.OperationCode;
        var res = context.ResourceCode;

        var inlineCandidates = await _db.Documents
            .Where(d => d.IsReleased && !d.IsObsolete)
            .Where(d =>
                // Match anything that mentions our part (or is unscoped on part)
                (d.PartNumber == null || d.PartNumber == part)
                && (d.Revision == null || d.Revision == rev)
                && (op == null || d.OperationCode == null || d.OperationCode == op)
                && (res == null || d.ResourceCode == null || d.ResourceCode == res))
            .ToListAsync(cancellationToken);

        var linkedIds = await _db.DocumentLinks
            .Where(l =>
                (l.Scope == DocumentScope.Part && l.ScopeKey == part)
                || (l.Scope == DocumentScope.Revision && l.ScopeKey == $"{part}/{rev}")
                || (op != null && l.Scope == DocumentScope.Operation && l.ScopeKey == $"{part}/{rev}/{op}")
                || (res != null && l.Scope == DocumentScope.Resource && l.ScopeKey == res))
            .Select(l => l.DocumentId)
            .ToListAsync(cancellationToken);

        var linked = await _db.Documents
            .Where(d => linkedIds.Contains(d.Id) && d.IsReleased && !d.IsObsolete)
            .ToListAsync(cancellationToken);

        var all = inlineCandidates
            .Concat(linked)
            .GroupBy(d => d.Id)
            .Select(g => g.First())
            .OrderBy(d => Specificity(d, part, rev, op, res))
            .ThenBy(d => d.Title)
            .ToList();

        return all;
    }

    /// <summary>Lower number = more specific = higher priority.</summary>
    private static int Specificity(Document d, string part, string rev, string? op, string? res)
    {
        if (op != null && d.PartNumber == part && d.Revision == rev && d.OperationCode == op) return 0;
        if (d.PartNumber == part && d.Revision == rev) return 1;
        if (d.PartNumber == part) return 2;
        if (res != null && d.ResourceCode == res) return 3;
        return 9;
    }
}
