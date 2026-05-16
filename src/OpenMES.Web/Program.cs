using Microsoft.EntityFrameworkCore;
using OpenMES.Application;
using OpenMES.Infrastructure;
using OpenMES.Infrastructure.Persistence;
using OpenMES.Infrastructure.Seeding;
using OpenMES.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOpenMesInfrastructure(builder.Configuration);
builder.Services.AddOpenMesApplication();

var app = builder.Build();

// Apply migrations + seed on startup so a fresh `docker compose up` + `dotnet run`
// gives operators something to look at. Failure is non-fatal in dev so the UI
// still boots when the DB isn't reachable yet.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<OpenMesDbContext>();
        await db.Database.MigrateAsync();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        await seeder.SeedIfEmptyAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration/seed failed on startup. The UI will still load; fix the database and reload.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
