using OpenMES.Domain.Entities;

namespace OpenMES.PluginAbstractions;

/// <summary>
/// Pulls document metadata from an upstream PLM / vault. The connector is
/// responsible for translating external schemas into <see cref="Document"/>
/// records that OpenMES can store and resolve.
/// </summary>
public interface IExternalDocumentConnector
{
    IAsyncEnumerable<Document> FetchDocumentsAsync(
        DateTime? changedSinceUtc = null,
        CancellationToken cancellationToken = default);
}
