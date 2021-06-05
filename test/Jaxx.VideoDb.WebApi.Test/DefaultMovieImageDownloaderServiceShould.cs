using Jaxx.VideoDb.WebCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.IO;

namespace Jaxx.VideoDb.WebApi.Test
{
    public class DefaultMovieImageDownloaderServiceShould
    {
        private readonly DefaultMovieImageDownloadService _movieImageDownloadService;

        public DefaultMovieImageDownloaderServiceShould()
        {
            var host = TestMovieDataServiceHost.Host().Build();
            host.StartAsync().Wait();
            _movieImageDownloadService = host.Services.GetService(typeof(IMovieImageDownloadService)) as DefaultMovieImageDownloadService;
        }

        [Fact]
        [Trait("Category", "OnlineWWW")]
        public async void DownloadImage()
        {
            var url = "https://www.google.com/images/branding/googlelogo/1x/googlelogo_color_272x92dp.png";
            var file = Path.GetRandomFileName();            

            var result = await _movieImageDownloadService.DownloadImageAsync(url, file);

            Assert.True(result.Item1, "Error downloading file");
            // Clear test run
            File.Delete(file);
            Assert.False(File.Exists(file), "Error deleting temp file.");
        }

        [Fact]
        [Trait("Category", "OnlineWWW")]
        public async void NotDownloadNonImage()
        {
            var url = "https://www.google.com/";
            var file = Path.GetRandomFileName();

            var result = await _movieImageDownloadService.DownloadImageAsync(url, file);

            Assert.False(result.Item1);            
        }
    }
}
