using AutoMapper;
using Jaxx.Images;
using Jaxx.WebGallery.DataModels;
using Jaxx.WebGallery.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Jaxx.WebGallery
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddGallery(this IServiceCollection services, IConfiguration configuration)
        {
            var dbConnectionString = configuration.GetValue<string>("WebGallery:Database:ConnectionString");

            services.AddDbContext<GalleryContext>(options =>
            {
                options.UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString)); ;
            });
            services.AddAutoMapper(typeof(WebGalleryMappingProfile));
            services.AddTransient<IGalleryService, DefaultGalleryService>();
            services.AddTransient<ImageStreamer>();
            services.AddSingleton(new GalleryConfiguration 
            { 
                AlbumThumbPath = configuration.GetValue<string>("WebGallery:AlbumThumbPath"),
                ImageThumbPath = configuration.GetValue<string>("WebGallery:ImageThumbPath"),
                ImagePath = configuration.GetValue<string>("WebGallery:ImagePath")
            });

            services.AddHostedService<FileWatcherHostedService>();
            
            return services;
        }
    }
}
