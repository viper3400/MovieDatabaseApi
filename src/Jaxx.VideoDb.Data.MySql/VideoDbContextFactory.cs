using Jaxx.VideoDb.Data.Context;
using Jaxx.WebApi.Shared;
using Microsoft.EntityFrameworkCore;
using System;

namespace Jaxx.VideoDb.Data.MySql
{
    /// <summary>
    /// Factory class for EmployeesContext
    /// </summary>
    public static class VideoDbContextFactory
    {
        public static VideoDbContext Create(string connectionString, IUserContextInformationProvider viewGroupProvider)
        {
            var optionsBuilder = new DbContextOptionsBuilder<VideoDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            //Ensure database creation
            var context = new VideoDbContext(optionsBuilder.Options, viewGroupProvider);
            //context.Database.EnsureCreated();

            return context;
        }
    }
}
