# OpenMES

OpenMES is an open-source, .NET-based **shop-floor execution portal** for manufacturing
operators, technicians, quality staff, and supervisors.

It sits **in front of** an existing ERP / MRP / database / document system and gives the
people on the floor a clean, focused interface for the work they actually do — opening
the right job, the right drawing, scanning the right material, and recording what
happened.

## What OpenMES is

- An operator-facing portal for the shop floor.
- A thin, opinionated layer over your existing systems of record.
- Event-based: production activity is captured as append-only events, not just status
  flags on a row.
- Extensible: the things that vary between companies (ERP, barcodes, documents) are
  behind interfaces in `OpenMES.PluginAbstractions`.

## What OpenMES is *not*

- It is **not** an ERP, MRP, or accounting system.
- It is **not** a replacement for a full heavyweight MES suite.
- It is **not** a document management system. Documents are referenced by path/URL.
- It is **not** a scheduling optimizer. Schedules are displayed, not solved.

## MVP scope

The first vertical slice covers:

1. Jobs (list, detail, status)
2. Parts and revisions
3. Resources / work centers
4. Documents linked to part / revision / operation / resource
5. Operator job page
6. Material lot scan + issue workflow
7. Production event logging (good / scrap / downtime)
8. Audit / event history (append-only)

Authentication is stubbed for now (placeholder roles: Operator, Technician, Quality,
Supervisor, Admin).

## Architecture

```
OpenMES.Web                 ← Blazor Web App (operator UI)
OpenMES.Application         ← use cases / services
OpenMES.Domain              ← entities, value objects, event types
OpenMES.Infrastructure      ← EF Core, PostgreSQL, file/URL access
OpenMES.Worker              ← background sync / export (stub)
OpenMES.PluginAbstractions  ← interfaces for company-specific integrations
```

Rules of the road:

- The UI never depends on ERP schemas. External data is normalized into OpenMES
  concepts before it reaches the UI.
- Production history is event-based and append-only.
- Domain has no dependencies on EF Core or ASP.NET.

See [`docs/architecture/overview.md`](docs/architecture/overview.md) for more detail.

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) (this repo targets `net10.0`).
  If only an older LTS is available, see the deviation notes in
  [`TASKS.md`](TASKS.md) / [`CHANGELOG.md`](CHANGELOG.md).
- Docker + Docker Compose (for local PostgreSQL).
- (Optional) `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`.

## Run locally

```bash
# 1. Start PostgreSQL
docker compose up -d

# 2. Restore + build
dotnet restore
dotnet build

# 3. Apply migrations (creates database schema)
dotnet ef database update \
  --project src/OpenMES.Infrastructure \
  --startup-project src/OpenMES.Web

# 4. Run the operator portal
dotnet run --project src/OpenMES.Web
```

Seed data is loaded automatically on first startup when the database is empty. You can
also re-seed from `/admin/seed-data`.

The connection string defaults to the value in
`src/OpenMES.Web/appsettings.Development.json`, which points at the Docker Compose
PostgreSQL instance. In production, set `ConnectionStrings__OpenMes` as an environment
variable.

## Run tests

```bash
dotnet test
```

Unit tests cover barcode parsing, document resolution, production event creation, and
material issue validation. Integration tests against a real database live in
`tests/OpenMES.IntegrationTests` (skipped automatically when PostgreSQL is unreachable).

## Contributing

This is a young project. The highest-leverage contributions right now:

- Real connector implementations against actual ERPs (in your own fork — keep them
  out of this repo unless they're generic).
- Better operator UX on tablet form factors.
- More barcode formats in `IBarcodeParser` implementations.
- Document resolvers for common PLM / vault systems.

Please open an issue describing the scenario before sending a large PR.

## Status

**Early MVP.** Domain model, EF Core persistence, seed data, and the operator vertical
slice (jobs → documents → scan → production → events) are in place. Production use is
not yet recommended.
