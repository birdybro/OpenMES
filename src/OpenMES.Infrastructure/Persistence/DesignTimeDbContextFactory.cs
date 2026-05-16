using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OpenMES.Infrastructure.Persistence;

/// <summary>
/// Used by <c>dotnet ef</c> tooling when there is no host to ask. Reads the
/// connection string from <c>OPENMES_CONNECTION</c> if set, otherwise falls
/// back to the docker-compose default.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OpenMesDbContext>
{
    public OpenMesDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("OPENMES_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=openmes;Username=openmes;Password=openmes";
        var options = new DbContextOptionsBuilder<OpenMesDbContext>()
            .UseNpgsql(cs)
            .Options;
        return new OpenMesDbContext(options);
    }
}
