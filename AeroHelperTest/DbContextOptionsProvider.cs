using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PROJETOESA.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroHelperTest
{
    public class AeroHelperContextFixture : IDisposable
    {
        public AeroHelperContext DbContext { get; set; }

        public AeroHelperContextFixture()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<AeroHelperContext>()
                          .UseSqlite(connection)
                          .Options;
            DbContext = new AeroHelperContext(options);
            DbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}