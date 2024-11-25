using Microsoft.EntityFrameworkCore;
using Prober.Entities;
using Prober.Options;

namespace Prober;

internal static class Settings
{
    public static async Task<WebApplication> InitApplicationNames(this WebApplication app, AppSettings settings)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DbContext>();
        var applicationsSet = db.Set<Application>();

        var existingApp = await applicationsSet.FirstOrDefaultAsync(a => a.Name == settings.Name);
        if (existingApp is not null)
            existingApp.Key = settings.Key;
        else
            applicationsSet.Add(new Application {Name = settings.Name, Key = settings.Key});

        await db.SaveChangesAsync();
        
        return app;
    }
}