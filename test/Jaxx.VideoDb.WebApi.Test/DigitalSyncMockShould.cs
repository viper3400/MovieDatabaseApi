using Jaxx.VideoDb.WebCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using Jaxx.VideoDb.Data.DatabaseModels;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;
using Jaxx.VideoDb.Data.Context;
using Microsoft.EntityFrameworkCore;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.WebApi.Shared.Models;
using System.Threading;

namespace Jaxx.VideoDb.WebApi.Test
{
    [Collection("AutoMapperCollection")]
    public class DigitalCopySyncMockShould : IDisposable
    {
        private readonly DigitalCopySync digitalCopySync;
        private readonly IMovieDataService movieDataService;
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ILogger logger;
        private MockFileSystem fileSystem;

        public DigitalCopySyncMockShould(ITestOutputHelper testOutputHelper)
        {
            TearUpFileSystem();

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            logger = loggerFactory.CreateLogger<DigitalCopySync>();

            var host = TestMovieDataServiceHost.Host()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IFileSystem>(fileSystem);
                })
                .Build();
            host.StartAsync().Wait();
            digitalCopySync = host.Services.GetService(typeof(DigitalCopySync)) as DigitalCopySync;
            movieDataService = host.Services.GetService(typeof(IMovieDataService)) as IMovieDataService;

            TearUpDatabase().Wait();
        }

        [Fact]
        public void SkipIfFilenameAlreadyUsedByOtherMovie()
        {
            var entriesWithFilename = new List<videodb_videodata>
            {
                new videodb_videodata { id = 4, title = "UnitTestMovie 1", filename = "V:\\UnitTestMovie 1\\UnitTestMovie 1.mkv"},
                new videodb_videodata { id = 5, title = "UnitTestMovie 2", filename = "V:\\UnitTestMovie 2\\UnitTestMovie 2.mkv"},
            };

            var result = digitalCopySync.GetAllFilesFromStorage("V:", "*.mkv");
            var fileInfos = new List<IFileInfo>
            {
                new MockFileInfo(fileSystem, @"V:\UnitTestMovie 2\UnitTestMovie 2.mkv"),
                new MockFileInfo(fileSystem, @"V:\UnitTestMovie 1\UnitTestMovie 1.mkv"),
                new MockFileInfo(fileSystem, @"V:\UnitTestMovie 4\UnitTestMovie 4.mkv")
            };

            var actual = digitalCopySync.ExcludeFileNamesAlreadyInUse(fileInfos, entriesWithFilename);
            Assert.Single(actual);
            Assert.Contains(@"V:\UnitTestMovie 4\UnitTestMovie 4.mkv", result.Select(i => i.FullName));

        }

        //[Fact(Skip ="DbContext not injected")]
        [Fact]
        [Trait("Category", "Online")]
        public void FindMatchingTitles()
        {
            var fileInfos = new List<IFileInfo>
            {
                new MockFileInfo(fileSystem, @"V:\UnitTestMovie 2\UnitTestMovie 2.mkv"),
                new MockFileInfo(fileSystem, @"V:\UnitTestMovie 1\UnitTestMovie 1.mkv"),
                new MockFileInfo(fileSystem, @"V:\UnitTestMovie 4\UnitTestMovie 4.mkv")
            };

            var actual = digitalCopySync.FindMatchingTitles("V:", "*.mkv");
            Assert.Equal(629, actual.Count());
            var x = actual.SelectMany(s => s.matchingFiles);
            Assert.Single(x.Where(m => m.FullName == @"V:\UnitTestMovie 4\UnitTestMovie 4.mkv"));
            Assert.Empty(x.Where(m => m.FullName == @"V:\UnitTestMovie 2\UnitTestMovie 2.mkv"));
            Assert.Empty(x.Where(m => m.FullName == @"V:\UnitTestMovie 1\UnitTestMovie 1.mkv"));

        }

        [Fact]
        [Trait("Category", "Online")]
        public void GetAllFilesFromStorage()
        {
            var result = digitalCopySync.GetAllFilesFromStorage("V:", "*.mkv");
            Assert.Equal(4, result.Count());
            Assert.Contains(@"V:\UnitTestMovie 2\UnitTestMovie 2.mkv", result.Select(i => i.FullName));
        }


        [Fact]
        [Trait("Category", "Online")]
        public void FindFilesWithoutDbEntries()
        {
            var orpahns = digitalCopySync.FindFilesWithoutDbEntries("V:", "*.mkv");
            Assert.Equal(2, orpahns.Count());
            Assert.Contains(@"V:\UnitTestMovie Orphan 1\UnitTestMovie Orphan 1.mkv", orpahns.Select(o => o.FullName));
            Assert.Contains(@"V:\UnitTestMovie 4\UnitTestMovie 4.mkv", orpahns.Select(o => o.FullName));
        }

        private void TearUpFileSystem()
        {
            // Create mocked file system
            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) },
                { @"V:\UnitTestMovie 1\UnitTestMovie 1.mkv", new MockFileData("movie 1") },
                { @"V:\UnitTestMovie 2\UnitTestMovie 2.mkv", new MockFileData("movie 2") },
                { @"V:\UnitTestMovie 4\UnitTestMovie 4.mkv", new MockFileData("movie 4") },
                { @"V:\UnitTestMovie Orphan 1\UnitTestMovie Orphan 1.mkv", new MockFileData("orpahn 1") },
                { @"V:\Filme\Was nützt die Liebe in Gedanken\Was nützt die Liebe in Gedanken.mkv", new MockFileData("Was nützt die ...") }
            });
        }

        private async Task<Task> TearUpDatabase()
        {
            var unitTestEntries = new List<MovieDataResource>
            {
                new MovieDataResource { title = "UnitTestMovie 1", filename = @"V:\UnitTestMovie 1\UnitTestMovie 1.mkv", diskid="R50F1D01", owner_id = 3, mediatype = 1, Genres = new List<MovieDataGenreResource> { new MovieDataGenreResource { Id = 4} }},
                new MovieDataResource { title = "UnitTestMovie 2", filename = @"V:\UnitTestMovie 2\UnitTestMovie 2.mkv", diskid="R50F1D02", owner_id = 3, mediatype = 1, Genres = new List<MovieDataGenreResource> { new MovieDataGenreResource { Id = 4} }},
                new MovieDataResource { title = "UnitTestMovie 3", filename = @"V:\UnitTestMovie 3\UnitTestMovie 3.mkv", diskid="R50F1D03", owner_id = 3, mediatype = 1, Genres = new List<MovieDataGenreResource> { new MovieDataGenreResource { Id = 4} }},
                new MovieDataResource { title = "UnitTestMovie 4", diskid="R50F1D04", owner_id = 3, mediatype = 16, Genres = new List<MovieDataGenreResource> { new MovieDataGenreResource { Id = 4} }}
            }; 

            foreach(var entry in unitTestEntries)
            {
                var result = await movieDataService.GetMovieDataAsync(null, new PagingOptions { Limit = 100, Offset = 0 }, new MovieDataOptions { Title = entry.title, ExactMatch = true }, new CancellationToken());
                if (result.TotalSize == 0)
                {
                    await movieDataService.CreateMovieDataAsync(entry, new CancellationToken());
                }
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            var unitTestEntries = new List<MovieDataResource>
            {
                new MovieDataResource { title = "UnitTestMovie 1", filename = @"V:\UnitTestMovie 1\UnitTestMovie 1.mkv" },
                new MovieDataResource { title = "UnitTestMovie 2", filename = @"V:\UnitTestMovie 2\UnitTestMovie 2.mkv" },
                new MovieDataResource { title = "UnitTestMovie 3", filename = @"V:\UnitTestMovie 3\UnitTestMovie 3.mkv" },
                new MovieDataResource { title = "UnitTestMovie 4"},
            };

            foreach (var entry in unitTestEntries)
            {
                var result = movieDataService.GetMovieDataAsync(null, new PagingOptions { Limit = 100, Offset = 0 }, new MovieDataOptions { Title = entry.title, ExactMatch = true }, new CancellationToken()).Result;
                if (result.TotalSize > 0)
                {
                    movieDataService.DeleteMovieDataAsync(result.Items.FirstOrDefault().id, new CancellationToken()).Wait();
                }
            }
        }
    }
}
