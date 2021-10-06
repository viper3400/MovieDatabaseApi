using Jaxx.VideoDb.WebCore.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Jaxx.VideoDb.WebApi.Test
{
    public class HostTest
    {
        private readonly IMovieDataService movieDataService;
        public HostTest()
        {
            var host = TestMovieDataServiceHost.Host().Build();
            host.StartAsync().Wait();
            movieDataService = host.Services.GetService(typeof(IMovieDataService)) as IMovieDataService;
        }

        [Fact]
        public async void GetConfigurationFromHost()
        {
            using var host = TestMovieDataServiceHost.Host().Build();
            await host.StartAsync();
            var configuration = host.Services.GetService(typeof(IConfiguration)) as IConfiguration;
            var username = configuration["TestUserName"];
            Assert.Equal("jan.graefe", username);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void GetNextFreeDiskId()
        {
            //using var host = TestMovieDataServiceHost.Host().Build();
            //await host.StartAsync();
            //var movieDataService = host.Services.GetService(typeof(IMovieDataService)) as IMovieDataService;
            var actual = await movieDataService.GetNextFreeDiskId("R12F5");
            Assert.Equal("R12F5D04", actual);
        }

        [Fact]
        [Trait("Category", "Online")]
        public async void ReturnAllGenres()
        {
            //using var host = TestMovieDataServiceHost.Host().Build();
            //await host.StartAsync();
            //var movieDataService = host.Services.GetService(typeof(IMovieDataService)) as IMovieDataService;
            var actual = await movieDataService.GetAllGenres(new System.Threading.CancellationToken());
            Assert.Equal(26, actual.Count());
            Assert.Equal("Action", actual.FirstOrDefault(i => i.Id == 1).Name);
            Assert.Equal("Romance", actual.FirstOrDefault(i => i.Id == 14).Name);
        }

    }
}
