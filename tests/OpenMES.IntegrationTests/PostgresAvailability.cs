namespace OpenMES.IntegrationTests;

/// <summary>
/// Placeholder. Real Postgres-backed tests will land here once
/// Testcontainers / a shared compose harness is wired in.
/// </summary>
public class PostgresAvailability
{
    [Fact(Skip = "Integration test harness not implemented yet — requires a live PostgreSQL.")]
    public void PlaceholderUntilHarnessLands() => Assert.True(true);
}
