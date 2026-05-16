using OpenMES.Domain.Entities;

namespace OpenMES.PluginAbstractions;

/// <summary>
/// Forwards persisted <see cref="ProductionEvent"/> records to an external
/// system (ERP write-back, data warehouse, MQTT topic, …). Called after the
/// event is committed; should not throw — log and swallow.
/// </summary>
public interface IProductionEventSink
{
    Task HandleAsync(ProductionEvent productionEvent, CancellationToken cancellationToken = default);
}
