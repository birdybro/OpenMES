# Getting started (developer)

This is a short crib sheet for setting up an OpenMES dev environment.

## Prerequisites

- .NET SDK 10
- Docker + Docker Compose
- Optional: `dotnet tool install --global dotnet-ef`

## First-time setup

```bash
docker compose up -d                      # PostgreSQL on :5432
dotnet restore
dotnet build
dotnet ef database update \
  --project src/OpenMES.Infrastructure \
  --startup-project src/OpenMES.Web
dotnet run --project src/OpenMES.Web
```

Browse to `http://localhost:5000` (or whatever Kestrel prints). Seed data is loaded
automatically when the database is empty, so the operator pages have content the
first time you open them.

## Running tests

```bash
dotnet test
```

Unit tests run in-process. Integration tests that need PostgreSQL are skipped if
the database is unreachable.

## Useful commands

```bash
# Create a new migration after a domain change
dotnet ef migrations add <Name> \
  --project src/OpenMES.Infrastructure \
  --startup-project src/OpenMES.Web

# Drop and recreate the schema (DESTROYS DATA)
dotnet ef database drop -f \
  --project src/OpenMES.Infrastructure \
  --startup-project src/OpenMES.Web

# Format the whole solution
dotnet format
```

## Where things live

| Layer            | Project                       |
|------------------|-------------------------------|
| UI (Blazor)      | `src/OpenMES.Web`             |
| Use cases        | `src/OpenMES.Application`     |
| Entities         | `src/OpenMES.Domain`          |
| EF / PostgreSQL  | `src/OpenMES.Infrastructure`  |
| Background work  | `src/OpenMES.Worker`          |
| Plugin interfaces| `src/OpenMES.PluginAbstractions` |
| Unit tests       | `tests/OpenMES.Domain.Tests`, `tests/OpenMES.Application.Tests` |
| Integration      | `tests/OpenMES.IntegrationTests` |
| Seed data        | `samples/seed-data`           |
