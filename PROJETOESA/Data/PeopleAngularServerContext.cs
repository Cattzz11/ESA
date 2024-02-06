using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PROJETOESA.Models;

namespace PeopleAngular.Server.Data
{
    public class PeopleAngularServerContext : IdentityDbContext<User>
    {
        public PeopleAngularServerContext(DbContextOptions<PeopleAngularServerContext> options)
            : base(options)
        {

        }

        public DbSet<Person> Person { get; set; } = default!;
    }
}
