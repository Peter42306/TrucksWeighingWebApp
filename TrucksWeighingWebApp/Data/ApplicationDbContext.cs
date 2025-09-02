using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<UserSession> UserSessions { get; set; }        
        public DbSet<Inspection> Inspections { get; set; }
        public DbSet<TruckRecord> TruckRecords { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserSession>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.LastSeenUtc);
                e.HasIndex(x => new { x.StartedUtc, x.UserId });
                e.Property(x => x.UserId).IsRequired();
            });

            builder.Entity<ApplicationUser>(e =>
            {
                e.HasIndex(x => x.CreatedAt);
            });

            builder.Entity<Inspection>(e =>
            {
                e.Property(x => x.DeclaredTotalWeight).HasPrecision(18, 3);
                e.Property(x => x.WeighedTotalWeight).HasPrecision(18, 3);
                e.Property(x => x.DifferenceWeight).HasPrecision(18, 3);
                e.Property(x => x.DifferencePercent).HasPrecision(18, 2);

                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TruckRecord>(e =>
            {
                e.Property(x => x.PlateNumber).HasMaxLength(32);
                e.Property(x => x.InitialWeight).HasPrecision(18, 3);
                e.Property(x => x.FinalWeight).HasPrecision(18, 3);
                e.Property(x => x.NetWeight).HasPrecision(18, 3);                
            });
        }
    }
}
