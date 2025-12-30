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
                .WithOne(u => u.Worker)
                .HasForeignKey<Worker>(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CarMake>()
                .HasOne(cm => cm.Creator)
                .WithMany(u => u.CarMakes)
                .HasForeignKey(cm => cm.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CarService>()
                .HasOne(cs => cs.Boss)
                .WithOne(u => u.CarService)
                .HasForeignKey<CarService>(cs => cs.BossId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Worker>()
                .HasMany(w => w.Accesses)
                .WithMany(r => r.Workers)
                .UsingEntity<Dictionary<string, object>>(
                    "WorkerAccesses",
                    j => j.HasOne<Access>().WithMany().HasForeignKey("AccessId"),
                    j => j.HasOne<Worker>().WithMany().HasForeignKey("WorkerId")
                );

            builder.Entity<PartsFolder>()
                .HasOne(f => f.Parent)
                .WithMany(c => c.FolderChildren)
                .HasForeignKey(f => f.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Part>()
                .HasOne(p => p.Parent)
                .WithMany(f => f.PartsChildren)
                .HasForeignKey(p => p.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Client>()
                .HasOne(c => c.CarService)
                .WithMany(s => s.Clients)
                .HasForeignKey(c => c.CarServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<JobPart>()
                .HasKey(jp => new { jp.JobId, jp.PartId });

            builder.Entity<JobPart>()
                .HasOne(jp => jp.Job)
                .WithMany(j => j.JobParts)
                .HasForeignKey(jp => jp.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<JobPart>()
                .HasOne(jp => jp.Part)
                .WithMany(p => p.JobParts)
                .HasForeignKey(jp => jp.PartId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<CarService> CarServices { get; set; } = null!;
        public DbSet<Worker> Workers { get; set; } = null!;
        public DbSet<Access> Accesses { get; set; } = null!;
        public DbSet<JobType> JobTypes { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<CarMake> CarMakes { get; set; } = null!;
        public DbSet<CarModel> CarModels { get; set; } = null!;
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Car> Cars { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<PartsFolder> PartsFolders { get; set; } = null!;
        public DbSet<Part> Parts { get; set; } = null!;
        public DbSet<JobPart> JobParts { get; set; } = null!;
        public DbSet<WorkerSchedule> WorkerSchedules { get; set; } = null!;
        public DbSet<WorkerLeave> WorkerLeaves { get; set; } = null!;
    }
}