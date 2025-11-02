using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GarageControl.Infrastructure.Data.Models;

namespace GarageControl.Infrastructure.Data
{
    public class GarageControlDbContext : IdentityDbContext<User>
    {
        public GarageControlDbContext(DbContextOptions<GarageControlDbContext> options)
            : base(options)
        {
        }

        public DbSet<CarService> CarServices { get; set; } = null!;
    }
}