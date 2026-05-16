# OpenMES — Task List

This file tracks the work needed to bring OpenMES from an empty repository to a
usable shop-floor MVP. Update it (and `CHANGELOG.md`) with every meaningful change.

Legend: `[ ]` not started · `[~]` in progress · `[x]` done

---

## Phase 0 — Repository bootstrap

- [x] Add `README.md` describing scope, architecture, prerequisites, and how to run.
  - _Acceptance:_ A new contributor can read it and know what OpenMES is and is not.
- [x] Add `CHANGELOG.md` (Keep-a-Changelog style).
  - _Acceptance:_ Has `Unreleased` section with Added/Changed/Fixed/Notes headings.
- [x] Add `TASKS.md` (this file) with phased checkboxes.
- [x] Add `docs/architecture/overview.md`.
- [x] Add `.gitignore` for .NET, Rider/VS, Docker.
- [x] Commit the bootstrap docs.

## Phase 1 — Domain model

- [x] Create solution and projects per the agreed layout.
- [x] Add entities: `Job`, `Part`, `PartRevision`, `Operation`, `Resource`,
      `ResourceScheduleEntry`, `Document`, `DocumentLink`, `MaterialLot`,
      `JobMaterialIssue`, `ProductionEvent`, `QualityCheck`, `QualityResult`,
      `ScanEvent`, user/role placeholders.
  - _Acceptance:_ Each entity has a clear primary key and the relationships
    implied by the MVP workflow.
- [x] Add `ProductionEventType` enum (JobCreated → BarcodeScanned).
- [x] Add plugin interfaces in `OpenMES.PluginAbstractions`.
  - _Acceptance:_ `IBarcodeParser`, `IDocumentResolver`,
    `IMaterialValidationRule`, `IProductionEventSink`, `IExternalJobConnector`,
    `IExternalDocumentConnector` all defined and documented.

## Phase 2 — Database and seed data

- [x] Add `OpenMesDbContext` with EF Core + Npgsql.
- [x] Add entity configurations (keys, indexes, lengths).
- [x] Add a design-time `DbContextFactory` for `dotnet ef` tooling.
- [x] Add the initial migration.
- [x] Add `docker-compose.yml` for PostgreSQL.
- [x] Add seed data: 3 resources, 5 parts (with revisions), several jobs,
      documents linked to part/revision/operation, material lots, sample
      production events.
- [x] Apply seed automatically on first startup when the database is empty.

## Phase 3 — Operator UI vertical slice (Blazor)

- [x] `/` — landing / dashboard with quick links.
- [x] `/resources` and `/resources/{id}` — work-center list and detail.
- [x] `/jobs` and `/jobs/{id}` — job list and detail with status.
- [x] `/jobs/{id}/documents` — resolved documents for the job.
- [x] `/jobs/{id}/scan` — barcode entry / material lot scan.
- [x] `/jobs/{id}/production` — record good qty, scrap, downtime.
- [x] `/jobs/{id}/events` — append-only event history view.
- [x] `/admin/documents` — list documents with their linkage.
- [x] `/admin/seed-data` — trigger re-seed (dev only).
- [x] Use large, operator-readable controls and clear status indicators.

## Phase 4 — Document resolver

- [x] Built-in `DocumentResolver` that, given job + part revision + operation +
      resource, returns released non-obsolete documents in priority order.
  - _Acceptance:_ More specific links (operation > revision > part > resource)
    rank higher; obsolete/un-released documents are excluded.

## Phase 5 — Barcode / material issue workflow

- [x] `SimpleBarcodeParser` recognizing `JOB:`, `PART:`, `LOT:|QTY:`, `EMP:`.
- [x] `ScanService` that parses, validates, and produces a `ScanEvent`.
- [x] `MaterialIssueService` that creates a `JobMaterialIssue` and a
      `MaterialIssued` production event, validating the lot exists and has qty.

## Phase 6 — Production event tracking

- [x] `ProductionEventService` writing append-only events; never overwrites.
- [x] Helpers for `GoodQuantityReported`, `ScrapQuantityReported`,
      `DowntimeStarted`, `DowntimeEnded`, lifecycle events.
- [x] Job event history page reads back chronologically (newest first).

## Phase 7 — Quality checks

- [x] Quality check definition tied to operation.
  - _Acceptance:_ Domain `QualityCheck` references `Operation`; seed data
    attaches numeric / pass-fail / visual checks to the inspection / final
    ops.
- [x] Operator UI for recording results.
  - _Acceptance:_ `/jobs/{id}/quality` lists checks grouped by operation
    with type-appropriate input (numeric input + spec hint, pass/fail
    buttons, visual confirm), and shows recorded results below.
- [x] `QualityCheckCompleted` event emitted on save.
  - _Acceptance:_ Notes carry `PASS` / `FAIL`; failures also carry
    `ReasonCode = "QC-FAIL"`.

## Phase 8 — Connectors and plugins

- [x] Interfaces defined (Phase 1).
- [ ] Reference implementation of `IExternalJobConnector` against a CSV or SQL
      sample (deferred — out of MVP scope).
- [ ] Reference implementation of `IExternalDocumentConnector` against a
      filesystem folder (deferred).
- [ ] Dynamic plugin loading (DLL discovery) — explicitly deferred.

## Phase 9 — Packaging / deployment

- [ ] Multi-stage `Dockerfile` for `OpenMES.Web`.
- [ ] `docker-compose.yml` profile that runs Web + PostgreSQL together.
- [ ] Production configuration guidance in README (env vars, reverse proxy).

---

## Cross-cutting

- [x] xUnit tests for barcode parsing, document resolution, production event
      creation rules, and material issue validation.
- [ ] Integration tests (Testcontainers / docker-compose) — scaffolded only.
- [x] Run `dotnet format` / `dotnet build` / `dotnet test` before committing.
