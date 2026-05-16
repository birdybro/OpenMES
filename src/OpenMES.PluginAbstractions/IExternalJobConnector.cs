using OpenMES.Domain.Entities;

namespace OpenMES.PluginAbstractions;

/// <summary>
/// Pulls jobs from an upstream system (ERP / MRP) and yields them as OpenMES
/// <see cref="Job"/> entities. The connector is responsible for translating
/// external schemas — no external types should leak out of this interface.
/// </summary>
public interface IExternalJobConnector
{
    IAsyncEnumerable<Job> FetchJobsAsync(
        DateTime? changedSinceUtc = null,
        CancellationToken cancellationToken = default);
}
