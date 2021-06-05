using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jaxx.WebGallery.Test
{
    public static class DefaultTestHost
    {
        public static IConfiguration Configuration { get; private set; }
        
        public static IHostBuilder Host()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("testsettings.json");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    Configuration = hostContext.Configuration;
                    services.AddGallery(Configuration);
                    //var dbConnectionString = Configuration["WebGallery:Database:ConnectionString"];
                    
                    //services.AddAutoMapper(typeof(WebGalleryMappingProfile));
                    //services.AddDbContext<GalleryContext>(options =>
                    //{
                    //    options.UseMySql(dbConnectionString);
                    //});
                    //services.AddTransient<DefaultGalleryService>();
                });

            return hostBuilder;

        }
    }
}
