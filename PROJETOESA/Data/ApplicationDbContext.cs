﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PROJETOESA.Data
{
<<<<<<< Updated upstream
    public class ApplicationDbContext : IdentityDbContext
=======
    public class PeopleAngularServerContext : IdentityDbContext<User>
>>>>>>> Stashed changes
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
