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
        public DbSet<Country> Countries { get; set; }
        public DbSet<Airport> Airport { get; set; }
        public DbSet<Carrier> Carrier { get; set; }
    }
}
