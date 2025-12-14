using GarageControl.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GarageControl.Shared.Constants;

namespace GarageControl.Infrastructure.Data.Seeding
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<GarageControlDbContext>();

            if (await context.Accesses.AnyAsync())
            {
                return;
            }

            var accesses = new List<Access>
            {
                new Access { Name = "Orders" },
                new Access { Name = "Parts Stock" },
                new Access { Name = "Workers" },
                new Access { Name = "Job Types" },
                new Access { Name = "Clients" },
                new Access { Name = "Service Details" }
            };

            await context.Accesses.AddRangeAsync(accesses);
            await context.SaveChangesAsync();
        }
    }
}
