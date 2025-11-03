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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CarService>()
                .HasOne(cs => cs.Boss)
                .WithMany()
                .HasForeignKey(cs => cs.BossId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Worker>()
                .HasOne(w => w.CarService)
                .WithMany(cs => cs.Workers)
                .HasForeignKey(w => w.CarServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Worker>()
                .HasOne(w => w.User)
                .WithOne()
                .HasForeignKey<Worker>(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<CarService> CarServices { get; set; } = null!;
        public DbSet<Worker> Workers { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<JobType> JobTypes { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<CarMake> CarMakes { get; set; } = null!;
        
    }
}