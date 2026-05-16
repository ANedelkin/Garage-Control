using Microsoft.EntityFrameworkCore;
using GarageControl.Infrastructure.Data;
using GarageControl.Infrastructure.Data.Seeding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GarageControl.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task SeedDatabaseAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            try
            {
                var context = serviceProvider.GetRequiredService<GarageControlDbContext>();
                await context.Database.MigrateAsync();
                await DbSeeder.SeedAsync(serviceProvider);
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred seeding the DB.");
            }
        }
    }
}
