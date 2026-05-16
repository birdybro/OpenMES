# Changelog

All notable changes to OpenMES are documented in this file.

The format is loosely based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project will adhere to [Semantic Versioning](https://semver.org/spec/v2.0.0.html)
once it reaches a 1.0 release.

## [Unreleased]

### Added
- Quality checks vertical slice (Phase 7):
  - `QualityService` lists checks scoped to a job's part-revision
    operations, lists results, and records new results. Numeric values are
    auto-evaluated against the check's `Min` / `Max`; pass/fail checks
    require an explicit selection; visual / text default to pass; an
    explicit `PassOverride` always wins.
  - Each recorded result emits a `QualityCheckCompleted` production event
    (notes carry `PASS` / `FAIL`; failures set `ReasonCode = "QC-FAIL"`).
  - `/jobs/{id}/quality` operator page groups checks by operation,
    renders type-appropriate input, and shows the result history.
  - `JobDetail` gains a "🔍 Quality" button alongside the other actions.
  - Seed data attaches numeric / pass-fail / visual checks to the
    inspection / final ops of BRK-100, SHF-220, and ASM-900.
- Initial documentation: `README.md`, `TASKS.md`, `docs/architecture/overview.md`.
- `.gitignore` for .NET / IDE / Docker artifacts.
- .NET 10 solution with the agreed project layout:
  `OpenMES.Web`, `OpenMES.Application`, `OpenMES.Domain`, `OpenMES.Infrastructure`,
  `OpenMES.Worker`, `OpenMES.PluginAbstractions`, plus matching test projects.
- Domain entities: Job, Part, PartRevision, Operation, Resource,
  ResourceScheduleEntry, Document, DocumentLink, MaterialLot, JobMaterialIssue,
  ProductionEvent, QualityCheck, QualityResult, ScanEvent, User/Role placeholders.
- `ProductionEventType` enum covering JobCreated → BarcodeScanned.
- Plugin abstractions: `IBarcodeParser`, `IDocumentResolver`,
  `IMaterialValidationRule`, `IProductionEventSink`, `IExternalJobConnector`,
  `IExternalDocumentConnector`.
- Built-in `SimpleBarcodeParser` recognizing `JOB:`, `PART:`, `LOT:|QTY:`, `EMP:`.
- Application services for jobs, document resolution, scans, material issue,
  production events, and event history.
- EF Core `OpenMesDbContext` with PostgreSQL provider and an initial migration.
- `docker-compose.yml` providing a local PostgreSQL instance.
- Seed data: 3 resources, 5 parts with revisions, several jobs, sample documents,
  material lots, sample production events.
- Blazor Web App UI with operator pages for resources, jobs, documents, scanning,
  production, and event history, plus an admin seed-data page.
- xUnit tests for barcode parsing, document resolution, production event creation,
  and material issue validation.

### Changed
- _(none yet)_

### Fixed
- _(none yet)_

### Notes
- Target framework is `net10.0`. .NET 10 SDK is required to build.
- Authentication is intentionally stubbed; real identity will be added in a later
  phase.
- Plugin loading is **not** dynamic yet — interfaces exist but implementations are
  registered in code.
