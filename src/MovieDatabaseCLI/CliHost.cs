using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.WebCore;
using Jaxx.VideoDb.WebCore.Services;
using Jaxx.WebApi.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieDatabaseCLI
{
    public static class CliHost
    {
        public static IHostBuilder Host()
        {
            var hostBuilder = new HostBuilder()
            //.UseEnvironment("UnitTests")
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
               // logging.SetMinimumLevel(LogLevel.Trace);

            })
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
               // services.AddVideoDb(Configuration);

                services.AddHostedService<Worker>();

            });

            return hostBuilder;
        }
        public static string UserName { get; private set; }

        public static string UserPassword { get; private set; }
        public static string ViewGroup { get; private set; }
        public static IConfiguration Configuration { get; private set; }
        //public static ServiceProvider TestServiceDIProvider { get; private set; }
        public static string ApiMasterKey { get; private set; }
        public static MovieDataServiceOptions MovieDataServiceOptions { get; private set; }
        public static DefaultMovieImageDownloadService TestMovieImageDownloadService { get; private set; }
    }
}

