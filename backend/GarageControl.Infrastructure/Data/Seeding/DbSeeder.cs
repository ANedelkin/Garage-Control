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

            var accesses = new List<string>
            {
                "Orders",
                "Parts Stock",
                "Workers",
                "Job Types",
                "Clients",
                "Service Details",
                "Makes and Models",
                "Cars"
            };

            foreach (var accessName in accesses)
            {
                 if (!await context.Accesses.AnyAsync(a => a.Name == accessName))
                 {
                     await context.Accesses.AddAsync(new Access { Name = accessName });
                 }
            }

            await context.SaveChangesAsync();
        }
    }
}
