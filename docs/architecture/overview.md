# Architecture overview

OpenMES is intentionally **small**. It is a thin, opinionated portal sitting in front
of whatever ERP / MRP / document system a manufacturer already runs.

This document describes the layering, the data flow, and the boundaries that exist so
that company-specific integration work can be done in plug-in projects rather than by
forking the core.

## Layering

```
┌────────────────────────────────────────────────────────────┐
│                       OpenMES.Web                          │
│   Blazor Web App. Operator + admin pages. No EF, no ERP.   │
└─────────────────────────┬──────────────────────────────────┘
                          │
┌─────────────────────────▼──────────────────────────────────┐
│                    OpenMES.Application                     │
│   Use cases / services: jobs, documents, scans,            │
│   material issue, production events. Depends only on       │
│   Domain + PluginAbstractions.                             │
└─────────────────────────┬──────────────────────────────────┘
                          │
┌─────────────────────────▼──────────────────────────────────┐
│                       OpenMES.Domain                       │
│   Entities, value objects, event-type enum.                │
│   No dependencies on EF, ASP.NET, or any external system.  │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│                  OpenMES.Infrastructure                    │
│   EF Core DbContext, Npgsql, repositories, seeders,        │
│   filesystem / URL document access, built-in plugin        │
│   implementations.                                         │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│                OpenMES.PluginAbstractions                  │
│   Interfaces only. No dependencies. Reference from any     │
│   layer that needs to talk to "the outside".               │
└────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────┐
│                       OpenMES.Worker                       │
│   Background sync / export. Hosted service host.           │
│   Empty stub today; will grow when external connectors     │
│   are added.                                                │
└────────────────────────────────────────────────────────────┘
```

The dependency arrows only ever point **inward**:

- `Web` → `Application` → `Domain`
- `Web`, `Worker` → `Infrastructure` (for DI registration only)
- `Infrastructure` → `Application` + `Domain` + `PluginAbstractions`
- Nothing depends on `Web`.
- Nothing in `Domain` depends on anything outside `Domain`.

## Why event-based production history

Production reality on the shop floor is messy: jobs are paused mid-cycle, an
operator scraps three of five parts, downtime starts before anyone updates a status
field. Modeling production as a stream of immutable `ProductionEvent` records
captures all of that without losing history.

Aggregate state (a job's current quantity good, scrap, run/idle status) is derived
from the event stream. The current row on `Job` is a convenient cache for the UI,
not the source of truth.

## Plug-in surface

The interfaces in `OpenMES.PluginAbstractions` define the only places where
company-specific code is expected:

| Interface                     | Responsibility                                              |
|-------------------------------|-------------------------------------------------------------|
| `IBarcodeParser`              | Decode a raw scan into a typed payload (job, part, lot, …). |
| `IDocumentResolver`           | Given a job context, return the relevant documents.         |
| `IMaterialValidationRule`     | Decide whether a lot may be issued to a job/operation.      |
| `IProductionEventSink`        | Forward produced events to an external system (ERP, BI).    |
| `IExternalJobConnector`       | Pull job orders from an upstream ERP/MRP.                    |
| `IExternalDocumentConnector`  | Pull document metadata from a PLM/vault.                     |

For the MVP, dynamic plugin discovery is **not** implemented. Implementations are
registered with the DI container directly. This avoids premature framework design.

## Document resolution

Documents are stored as records with a path/URL and several optional scoping
fields: `PartNumber`, `Revision`, `OperationCode`, `ResourceCode`, plus
`EffectiveDate`, `IsReleased`, `IsObsolete`. The built-in `DocumentResolver`
ranks results so that more specific links win — an operation-specific drawing
beats a part-level one, which beats a resource-level safety doc.

## Authentication

Authentication is a placeholder. The portal is intended to run inside a plant
network behind existing SSO. Roles (`Operator`, `Technician`, `Quality`,
`Supervisor`, `Admin`) are defined as constants so future authorization can hook
in without churn.

## What this architecture deliberately avoids

- **Microservices.** A plant intranet does not need them.
- **Kubernetes.** Docker Compose on a single host is enough for the MVP.
- **A scheduler.** Schedules come from upstream; OpenMES displays them.
- **A document management system.** OpenMES references documents; it does not
  store, version, or release them.
- **Dynamic plugin loading.** The interfaces are stable; the loader can be added
  later without breaking consumers.
