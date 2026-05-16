using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenMES.Application.Abstractions;
using OpenMES.Infrastructure.Persistence;
using OpenMES.Infrastructure.Seeding;

namespace OpenMES.Infrastructure;

public static class DependencyInjection
{
    public const string DefaultConnectionStringName = "OpenMes";

    public static IServiceCollection AddOpenMesInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = DefaultConnectionStringName)
    {
        var cs = configuration.GetConnectionString(connectionStringName)
            ?? Environment.GetEnvironmentVariable("OPENMES_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=openmes;Username=openmes;Password=openmes";

        services.AddDbContext<OpenMesDbContext>(opts => opts.UseNpgsql(cs));

        services.AddScoped<IOpenMesDb, OpenMesDbAdapter>();
        services.AddScoped<DataSeeder>();
        return services;
    }
}
