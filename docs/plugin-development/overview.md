# Writing plugins for OpenMES

The MVP does not load plugins dynamically. To extend OpenMES today, reference
`OpenMES.PluginAbstractions` from your own assembly, implement one of the
interfaces, and register it in the DI container in `Program.cs`.

## Available interfaces

### `IBarcodeParser`

Decode a raw scanned string into a typed payload. The built-in
`SimpleBarcodeParser` understands:

```
JOB:10001
PART:ABC-123
LOT:RESIN-BLACK|QTY:500
EMP:12345
```

Implementations are tried in registration order; the first one that returns a
non-null result wins.

### `IDocumentResolver`

Given a `DocumentResolutionContext` (job, part revision, operation, resource),
return the documents the operator should see. Multiple resolvers can be
registered; their results are merged and de-duplicated by document id.

### `IMaterialValidationRule`

A pluggable check that runs before a `MaterialLot` may be issued to a job.
Return a failure to block the issue, or success to allow it. Use this for things
like "this lot is on hold" or "this lot belongs to a different revision".

### `IProductionEventSink`

Forwards produced `ProductionEvent` records to an external system (ERP write-back,
data warehouse, MQTT topic). Sinks are called asynchronously after the event is
persisted.

### `IExternalJobConnector` / `IExternalDocumentConnector`

Pull jobs / documents from an upstream system. The connector translates the
external schema into OpenMES entities. Do not let external types leak above the
infrastructure layer.

## Registration example

```csharp
// in Program.cs
builder.Services.AddSingleton<IBarcodeParser, MyCompanyBarcodeParser>();
builder.Services.AddScoped<IMaterialValidationRule, NoExpiredLotsRule>();
builder.Services.AddScoped<IProductionEventSink, MyErpWriteBackSink>();
```

## Roadmap

- Dynamic discovery of plugin DLLs from a `plugins/` folder.
- Per-plugin configuration sections in `appsettings.json`.
- A health-check endpoint exposing connector status.

These are intentionally **not** in the MVP.
