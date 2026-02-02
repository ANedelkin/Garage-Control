using GarageControl.Infrastructure.Data.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GarageControl.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace GarageControl.Infrastructure.Data.Seeding
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            // Get the DB context
            var context = serviceProvider.GetRequiredService<GarageControlDbContext>();

            // Seed accesses
            var accesses = new List<string>
            {
                "Dashboard",
                "Orders",
                "Parts Stock",
                "Workers",
                "Job Types",
                "Clients",
                "Service Details",
                "Makes and Models",
                "Cars",
                "Activity Log",
            };

            var missingAccesses = accesses
                .Where(a => !context.Accesses.Any(e => e.Name == a))
                .Select(a => new Access { Name = a })
                .ToList();

            if (missingAccesses.Any())
            {
                await context.Accesses.AddRangeAsync(missingAccesses);
                await context.SaveChangesAsync();
            }

            // Seed Admin Role
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            const string adminRoleName = "Admin";

            if (!await roleManager.RoleExistsAsync(adminRoleName))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(adminRoleName));
                if (!roleResult.Succeeded)
                {
                    Console.WriteLine("Failed to create Admin role:");
                    foreach (var error in roleResult.Errors)
                        Console.WriteLine($"- {error.Description}");
                    return; // Stop seeding if role creation fails
                }
            }

            // Seed Admin User
            var adminEmail = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL");
            var adminPass = Environment.GetEnvironmentVariable("SEED_ADMIN_PASS");

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPass))
            {
                Console.WriteLine("Admin credentials not set in environment variables. Skipping admin seeding.");
                return;
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPass);
                if (!createResult.Succeeded)
                {
                    Console.WriteLine("Failed to create admin user:");
                    foreach (var error in createResult.Errors)
                        Console.WriteLine($"- {error.Description}");
                    return; // Stop further seeding if user creation fails
                }
            }

            // Ensure admin has Admin role
            if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
            {
                var addRoleResult = await userManager.AddToRoleAsync(adminUser, adminRoleName);
                if (!addRoleResult.Succeeded)
                {
                    Console.WriteLine("Failed to add Admin role to admin user:");
                    foreach (var error in addRoleResult.Errors)
                        Console.WriteLine($"- {error.Description}");
                }
            }

            Console.WriteLine("Seeding completed successfully.");
        }
    }
}
