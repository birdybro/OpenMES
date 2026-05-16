using Microsoft.Extensions.DependencyInjection;
using OpenMES.Application.Abstractions;
using OpenMES.Application.Barcodes;
using OpenMES.Application.Documents;
using OpenMES.Application.Jobs;
using OpenMES.Application.Materials;
using OpenMES.Application.Production;
using OpenMES.Application.Quality;
using OpenMES.Application.Resources;
using OpenMES.Application.Scanning;
using OpenMES.Application.Sync;
using OpenMES.PluginAbstractions;

namespace OpenMES.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOpenMesApplication(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();

        services.AddScoped<JobService>();
        services.AddScoped<ResourceService>();
        services.AddScoped<DocumentService>();
        services.AddScoped<ScanService>();
        services.AddScoped<MaterialIssueService>();
        services.AddScoped<ProductionEventService>();
        services.AddScoped<QualityService>();
        services.AddScoped<JobSyncService>();
        services.AddScoped<DocumentSyncService>();

        // Built-in plugin implementations. Consumers may register additional
        // ones; for resolvers/parsers each registered instance is consulted.
        services.AddSingleton<IBarcodeParser, SimpleBarcodeParser>();
        services.AddScoped<IDocumentResolver, DefaultDocumentResolver>();

        return services;
    }
}
