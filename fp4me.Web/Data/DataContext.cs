using System;
using System.Linq;
using fp4me.Web.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace fp4me.Web.Data
{
    // >dotnet ef migration add testMigration
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserPlan> UserPlans { get; set; }
        public DbSet<Park> Parks { get; set; }
        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<AttractionFastPassRequest> AttractionFastPassRequests { get; set; }
        public DbSet<AttractionFastPassRequestCheck> AttractionFastPassRequestChecks { get; set; }
        public DbSet<WaitlistCode> WaitlistCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                 .HasIndex(u => u.SMSNotificationPhoneNumber)
                 .IsUnique();

            builder.Entity<AttractionFastPassRequest>()
                .HasIndex(a => new { a.UserID, a.Status });

            builder.Entity<AttractionFastPassRequest>()
                .HasIndex(a => new { a.Status, a.Date, a.AttractionID });

            builder.Entity<AttractionFastPassRequest>()
                .HasIndex(a => new { a.UserID, a.Date, a.LastCheckTimestamp });

            builder.Entity<AttractionFastPassRequestCheck>()
                .HasIndex(a => a.AttractionFastPassStatus);

            builder.Entity<AttractionFastPassRequestCheck>()
                .HasIndex(a => new { a.AttractionID, a.Date });

        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            return base.SaveChanges();
        }

        private void updateUpdatedProperty<T>() where T : class
        {
            var modifiedSourceInfo =
                ChangeTracker.Entries<T>()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
        }

    }
}