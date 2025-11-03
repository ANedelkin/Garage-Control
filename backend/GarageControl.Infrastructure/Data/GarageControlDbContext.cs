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

            builder.Entity<CarMake>()
                .HasOne(cm => cm.Creator)
                .WithMany(u => u.CarMakes)
                .HasForeignKey(cm => cm.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CarService>()
                .HasOne(cs => cs.Boss)
                .WithMany(u => u.CarServices)
                .HasForeignKey(cs => cs.BossId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Worker>()
                .HasMany(w => w.Roles)
                .WithMany(r => r.Workers)
                .UsingEntity<Dictionary<string, object>>(
                    "WorkerRoles",
                    j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                    j => j.HasOne<Worker>().WithMany().HasForeignKey("WorkerId")
    );
        }

        public DbSet<CarService> CarServices { get; set; } = null!;
        public DbSet<Worker> Workers { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<JobType> JobTypes { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<CarMake> CarMakes { get; set; } = null!;
        public DbSet<CarModel> CarModels { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Car> Cars { get; set; } = null!;
    }
}