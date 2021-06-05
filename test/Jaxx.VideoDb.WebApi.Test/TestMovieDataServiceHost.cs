using AutoMapper;
using Jaxx.Images;
using Jaxx.VideoDb.Data.BusinessLogic;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.Data.Context;
using Jaxx.VideoDb.WebCore;
using Jaxx.VideoDb.WebCore.Infrastructure;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.VideoDb.WebCore.Services;
using Jaxx.WebApi.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebApi.Test
{
    public static class TestMovieDataServiceHost
    {
        public static IHostBuilder Host ()
        {
            var hostBuilder = new HostBuilder()
            .UseEnvironment("UnitTests")
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("ClientSecrets.json");
                config.AddJsonFile("testsettings.json");
                config.AddJsonFile("appsettings.json");
            })
            .ConfigureServices((hostContext, services) =>
            {
                Configuration = hostContext.Configuration;

                UserName = Configuration["TestUserName"];
                UserPassword = Configuration["TestUserPassword"];
                ViewGroup = Configuration["TestViewGroup"];
               
                ApiMasterKey = Configuration["Jwt:ApiMasterKey"];

                // Get movie data service configuration
                var deletedUserId = Configuration.GetValue<int>("MovieDataServiceOptions:DeletedUserId");
                var localCoverImagePath = Configuration.GetValue<string>("MovieDataServiceOptions:LocalCoverImagePath");
                var httpCoverImagePath = Configuration.GetValue<string>("MovieDataServiceOptions:HttpCoverImagePath");
                var localBackgroundImagePath = Configuration.GetValue<string>("MovieDataServiceOptions:LocalBackgroundImagePath");
                var mediaTypesFilter = Configuration.GetSection("MovieDataServiceOptions:MediaTypesFilter").Get<IEnumerable<int>>();

                MovieDataServiceOptions = new MovieDataServiceOptions
                {
                    DeletedUserId = deletedUserId,
                    LocalCoverImagePath = localCoverImagePath,
                    HttpCoverImagePath = httpCoverImagePath,
                    LocalBackgroundImagePath = localBackgroundImagePath,
                    MediaTypesFilter = mediaTypesFilter
                };

                services.AddScoped<IUserContextInformationProvider>(c => new DummyUserContextInformationProvider(UserName, ViewGroup));
                services.AddVideoDbMySql(Configuration);
                services.AddVideoDb(Configuration);

            });

            return hostBuilder;
        }
        public static string UserName { get; private set; }
        
        public static string UserPassword { get; private set; }
        public static string ViewGroup { get; private set; }
        public static IConfiguration Configuration { get; private set; }
        public static ServiceProvider TestServiceDIProvider { get; private set; }
        public static string ApiMasterKey { get; private set; }
        public static MovieDataServiceOptions MovieDataServiceOptions { get; private set; }
        public static DefaultMovieImageDownloadService TestMovieImageDownloadService { get; private set; }
    }
}
