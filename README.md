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

Unit tests cover barcode parsing, document resolution, production event creation,
material issue validation, quality check evaluation, the CSV / filesystem connectors,
and the sync services. Integration tests against a real database live in
`tests/OpenMES.IntegrationTests` (skipped automatically when PostgreSQL is unreachable).

## Deploying with Docker

A multi-stage `Dockerfile` for `OpenMES.Web` and a `full` compose profile that runs
Web + PostgreSQL together are included.

```bash
# Build + start the whole stack (Web on :8080, Postgres on :5432).
docker compose --profile full up -d --build

# Tail logs.
docker compose --profile full logs -f web

# Stop everything.
docker compose --profile full down
```

The default `docker compose up -d` still spins up **only** Postgres, so the local
`dotnet run` workflow keeps working unchanged.

### Configuration via environment variables

ASP.NET config maps `__` to `:` in env-var names. The variables OpenMES looks for:

| Variable                                       | Purpose                                                        |
|------------------------------------------------|----------------------------------------------------------------|
| `ASPNETCORE_ENVIRONMENT`                       | `Production` (default in container) or `Development`           |
| `ASPNETCORE_URLS`                              | Kestrel bind, e.g. `http://+:8080`                             |
| `ConnectionStrings__OpenMes`                   | PostgreSQL connection string                                   |
| `OpenMes__Connectors__CsvJobsPath`             | Path to the CSV the `CsvJobConnector` will read                |
| `OpenMes__Connectors__FileSystemDocumentsRoot` | Folder the `FileSystemDocumentConnector` will walk             |

The bundled compose file sets the connector paths to `/app/connectors/jobs.csv` and
`/app/connectors/documents` and bind-mounts the sample inputs from `samples/` so the
demo "just works." For a real deployment, replace those bind mounts with your own
folder (or a named volume populated by the ERP / vault export).

### Putting Web behind a reverse proxy (TLS)

The container speaks plain HTTP on `:8080`. For anything real, terminate TLS in
front (nginx, Caddy, Traefik) and forward to `http://openmes-web:8080`. A bare-bones
nginx site config:

```nginx
server {
  listen 443 ssl http2;
  server_name openmes.plant.local;
  ssl_certificate     /etc/ssl/openmes.crt;
  ssl_certificate_key /etc/ssl/openmes.key;

  location / {
    proxy_pass         http://openmes-web:8080;
    proxy_http_version 1.1;
    proxy_set_header   Host              $host;
    proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
    proxy_set_header   X-Forwarded-Proto $scheme;
    # Blazor Server uses WebSockets for the interactive circuit.
    proxy_set_header   Upgrade           $http_upgrade;
    proxy_set_header   Connection        "upgrade";
    proxy_read_timeout 3600;
  }
}
```

The image runs as the non-root `app` user. It does **not** include the EF tooling —
migrations are applied automatically on startup by `Program.cs`.

## Contributing

This is a young project. The highest-leverage contributions right now:

- Real connector implementations against actual ERPs (in your own fork — keep them
  out of this repo unless they're generic).
- Better operator UX on tablet form factors.
- More barcode formats in `IBarcodeParser` implementations.
- Document resolvers for common PLM / vault systems.

Please open an issue describing the scenario before sending a large PR.

## Status

**Early MVP.** Domain model, EF Core persistence, seed data, operator vertical slice
(jobs → documents → scan → production → events → quality), reference CSV / filesystem
connectors with a `/admin/sync` page, and a containerised deployment are in place.
Suitable for pilots; production-hardening (real auth, observability, backups,
secrets management) is the next round of work.
