using Microsoft.EntityFrameworkCore;
using System;

namespace Jaxx.WebApi.Shared.Identity.MySql
{
    public class IdentityDbContextFactory
    {
        public static IdentityDbContext Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            //Ensure database creation
            var context = new IdentityDbContext(optionsBuilder.Options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
