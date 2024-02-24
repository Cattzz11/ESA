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
                .HasKey(uf => new { uf.UserId, uf.FlightId });

            modelBuilder.Entity<City>()
                .HasMany(a => a.Flights)
                .WithOne()
                .HasForeignKey(f => f.OriginCityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<City>()
                .HasMany(a => a.Flights)
                .WithOne()
                .HasForeignKey(f => f.DestinationCityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.DestinationCity)
                .WithMany(a => a.Flights)
                .HasForeignKey(f => f.DestinationCityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Segment>()
                .HasOne(s => s.Flight)
                .WithMany(f => f.Segments)
                .HasForeignKey(s => s.FlightId);

            modelBuilder.Entity<AccompanyingPassenger>()
                .HasKey(ap => ap.Id);

            modelBuilder.Entity<AccompanyingPassenger>()
                .HasOne(ap => ap.UserFlight)
                .WithMany(uf => uf.AccompanyingPassengers)
                .HasForeignKey(ap => new { ap.UserId, ap.FlightId })
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
