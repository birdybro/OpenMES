# Sample seed data

The MVP seeds its demo dataset programmatically from
`src/OpenMES.Infrastructure/Seeding/DataSeeder.cs`. This folder is reserved for
future static fixtures (CSV / JSON imports, sample PDFs, etc.) so that the
seeder can grow without bloating the source tree.

For now: nothing to see here. Document paths in the seeded `Document` rows
point at `samples/docs/...` for illustration; those PDFs are intentionally
absent from the repo.
