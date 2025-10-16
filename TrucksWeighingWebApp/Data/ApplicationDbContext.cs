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
        public DbSet<UserLogo> UserLogos { get; set; }
        public DbSet<FeedbackTicket> FeedbackTickets { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FeedbackTicket>(e =>
            {
                e.HasKey(x => x.Id);

                // columns
                e.Property(x => x.ApplicationUserId).IsRequired();
                e.Property(x => x.UserEmail).IsRequired().HasMaxLength(256);
                e.Property(x => x.Message).IsRequired().HasMaxLength(4000);
                e.Property(x => x.AdminNotes).HasMaxLength(2000);
                e.Property(x => x.CreatedUtc).HasColumnType("timestamp without time zone").HasDefaultValueSql("timezone('utc', now())");

                // for sorting
                e.HasIndex(x => x.CreatedUtc);
                e.HasIndex(x => new { x.ApplicationUserId, x.CreatedUtc });

                // relation with user
                e.HasOne(x => x.ApplicationUser)
                    .WithMany(x => x.FeedbackTickets)
                    .HasForeignKey(x => x.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<UserLogo>(e =>
            {
                e.HasKey(x => x.Id);

                e.HasIndex(x => new { x.ApplicationUserId, x.Name }).IsUnique();

                e.HasOne(x => x.ApplicationUser)
                    .WithMany()
                    .HasForeignKey(x => x.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

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

                e.HasOne(x => x.ApplicationUser)
                    .WithMany()
                    .HasForeignKey(x => x.ApplicationUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.UserLogo)
                    .WithMany()
                    .HasForeignKey(x => x.UserLogoId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<TruckRecord>(e =>
            {
                e.HasOne(r => r.Inspection)
                    .WithMany(i => i.TruckRecords)
                    .HasForeignKey(r => r.InspectionId)
                    .OnDelete(DeleteBehavior.Cascade);
                                
                e.HasIndex(x => new { x.InspectionId, x.SerialNumber }).IsUnique();
                e.HasIndex(x => new { x.InspectionId, x.PlateNumber });

                e.Property(x => x.SerialNumber).IsRequired();
                e.Property(x => x.PlateNumber).HasMaxLength(64);


                e.Property(x => x.InitialWeight).HasPrecision(18, 3);
                e.Property(x => x.FinalWeight).HasPrecision(18, 3);                
            });
        }
    }
}
