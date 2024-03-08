using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PROJETOESA.Models;

namespace PROJETOESA.Data
{
    public class AeroHelperContext : IdentityDbContext<ApplicationUser>
    {
        public AeroHelperContext(DbContextOptions<AeroHelperContext> options)
            : base(options)
        {

        }

        //public DbSet<Person> Person { get; set; } = default!;

        public DbSet<PasswordRecoveryCode> PasswordRecoveryCodes { get; set; }

        public DbSet<ConfirmationCode> ConfirmationCodes { get; set; }
        public DbSet<Trip> Trip { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<Carrier> Carrier { get; set; }
        public DbSet<Segment> Segments { get; set; }
        public DbSet<AccompanyingPassenger> AccompanyingPassenger { get; set; }
        public DbSet<UserFlight> UserFlight { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserFlight>()
                .HasKey(uf => new { uf.UserId, uf.TripId });

            modelBuilder.Entity<Flight>(entity =>
            {
                entity.HasOne(f => f.OriginCity)
                    .WithMany()
                    .HasForeignKey(f => f.OriginCityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.DestinationCity)
                    .WithMany()
                    .HasForeignKey(f => f.DestinationCityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Segment>(entity =>
            {
                entity.HasOne(s => s.OriginCity)
                    .WithMany()
                    .HasForeignKey(s => s.OriginCityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.DestinationCity)
                    .WithMany()
                    .HasForeignKey(s => s.DestinationCityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Carrier)
                    .WithMany()
                    .HasForeignKey(s => s.CarrierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Flight)
                    .WithMany(f => f.Segments)
                    .HasForeignKey(s => s.FlightId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
