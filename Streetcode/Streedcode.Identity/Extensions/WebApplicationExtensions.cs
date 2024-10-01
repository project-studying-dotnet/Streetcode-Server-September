using Microsoft.EntityFrameworkCore;
using Streetcode.Identity.Data;

namespace Streedcode.Identity.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task ApplyMigrations(this WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            try
            {
                using var scope = app.Services.CreateScope();

                var streetcodeContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await streetcodeContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured during startup migration");
            }
        }
    }
}
