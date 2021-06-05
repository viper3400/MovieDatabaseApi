using AutoMapper;
using Jaxx.Images;
using Jaxx.VideoDb.Data.BusinessLogic;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.Data.Context;
using Jaxx.VideoDb.WebCore.Infrastructure;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.VideoDb.WebCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Jaxx.VideoDb.WebCore
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddVideoDbMySql(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("TESTDB_CONNECTIONSTRING");

            services.AddDbContext<VideoDbContext>(options =>
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            });

            return services;
        }

        public static IServiceCollection AddVideoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(DefaultAutomapperProfile));
            services.Configure<MovieMetaEngineOptions>(configuration.GetSection("DefaultMovieMetaEngineOptions"));

            // Get movie data service configuration
            var deletedUserId = configuration.GetValue<int>("MovieDataServiceOptions:DeletedUserId");
            var localCoverImagePath = configuration.GetValue<string>("MovieDataServiceOptions:LocalCoverImagePath");
            var httpCoverImagePath = configuration.GetValue<string>("MovieDataServiceOptions:HttpCoverImagePath");
            var localBackgroundImagePath = configuration.GetValue<string>("MovieDataServiceOptions:LocalBackgroundImagePath");
            var mediaTypesFilter = configuration.GetSection("MovieDataServiceOptions:MediaTypesFilter").Get<IEnumerable<int>>();

            services.AddSingleton(new MovieDataServiceOptions
            {
                DeletedUserId = deletedUserId,
                LocalCoverImagePath = localCoverImagePath,
                HttpCoverImagePath = httpCoverImagePath,
                LocalBackgroundImagePath = localBackgroundImagePath,
                MediaTypesFilter = mediaTypesFilter
            });


            var theMovieDbOptions = new TheMovieDbApi.TheMovieDbApiOptions
            {
                UseApi = configuration.GetValue<bool>("TheMovieDBApi:UseApi"),
                ApiKey = configuration.GetValue<string>("TheMovieDBApi:ApiKey"),
                ApiUrl = configuration.GetValue<string>("TheMovieDBApi:ApiUrl"),
                ApiImageBaseUrl = configuration.GetValue<string>("TheMovieDBApi:ApiImageBaseUrl"),
                ApiReferenceKey = configuration.GetValue<string>("TheMovieDBApi:ApiReferenceKey")
            };

            // Services that consume EF Core objects (DbContext) should be registered as Scoped
            // (see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection#registering-your-own-services)
            services.AddTransient<IMovieDataService, DefaultMovieDataService>();
            services.AddTransient<DiskIdGenerator>();
            services.AddTransient<ImageStreamer>();
            services.AddTransient<IInventoryService, InventoryService>();
            services.AddTransient<IMovieMetaService, DefaultMovieMetaService>();
            services.AddTransient<MovieMetaEngine.IMovieMetaSearch, OfdbParser.OfdbMovieMetaSearch>();
            services.AddTransient<MovieMetaEngine.IMovieMetaSearch, TheMovieDbApi.TheMovieDbApiHttpClient>();
            services.AddSingleton(theMovieDbOptions);
            //services.AddTransient<MovieMetaEngine.IMovieMetaSearch>(o => new TheMovieDbApi.TheMovieDbApiHttpClient(theMovieDbOptions));
            var genreMapperConfigFile = configuration.GetValue<string>("AppSettings:GenreMappingConfiguration");
            services.AddSingleton<XmlMapper.IObjectMapper>(o => new XmlMapper.ObjectMapper(genreMapperConfigFile));
            services.AddSingleton<IMovieMetaEngineRepository, DefaultMovieMetaEngineRepository>();
            services.AddTransient<IMovieImageDownloadService, DefaultMovieImageDownloadService>();
            services.AddTransient<DigitalCopySync>();

            return services;
        }
    }
}
