using OpenMES.Domain.Entities;

namespace OpenMES.PluginAbstractions;

public sealed record DocumentResolutionContext(
    int JobId,
    string PartNumber,
    string Revision,
    string? OperationCode = null,
    string? ResourceCode = null);

/// <summary>
/// Returns the documents the operator should see for a given job context.
/// Implementations should return released, non-obsolete documents, ranked from
/// most specific (operation) to least specific (resource / generic).
/// </summary>
public interface IDocumentResolver
{
    Task<IReadOnlyList<Document>> ResolveAsync(
        DocumentResolutionContext context,
        CancellationToken cancellationToken = default);
}
