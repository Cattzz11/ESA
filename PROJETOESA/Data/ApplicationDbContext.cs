﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PROJETOESA.Models;

namespace PROJETOESA.Data
{
    public class PeopleAngularServerContext : IdentityDbContext<ApplicationUser>
    {
        public PeopleAngularServerContext(DbContextOptions<PeopleAngularServerContext> options)
            : base(options)
        {

        }

        public DbSet<Person> Person { get; set; } = default!;
    }
}
