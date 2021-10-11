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

namespace Jaxx.VideoDb.WebApi.Test
{
    public class DigitalCopySyncMockShould
    {
        private readonly DigitalCopySync digitalCopySync;
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ILogger logger;
        private readonly MockFileSystem fileSystem;

        public DigitalCopySyncMockShould(ITestOutputHelper testOutputHelper)
        {

            fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"c:\myfile.txt", new MockFileData("Testing is meh.") },
                { @"c:\demo\jQuery.js", new MockFileData("some js") },
                { @"c:\demo\image.gif", new MockFileData(new byte[] { 0x12, 0x34, 0x56, 0xd2 }) },
                { @"V:\Movie 1\Movie 1.mkv", new MockFileData("movie 1") },
                { @"V:\Movie 2\Movie 2.mkv", new MockFileData("movie 2") },
                { @"V:\Movie 4\Movie 4.mkv", new MockFileData("movie 4") },
                { @"V:\Filme\Was nützt die Liebe in Gedanken\Was nützt die Liebe in Gedanken.mkv", new MockFileData("Was nützt die ...") }
            });

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
        }

        [Fact]
        public void SkipIfFilenameAlreadyUsedByOtherMovie()
        {
            var entriesWithFilename = new List<videodb_videodata>
            {
                new videodb_videodata { id = 4, title = "Movie 1", filename = "V:\\Movie 1\\Movie 1.mkv"},
                new videodb_videodata { id = 5, title = "Movie 2", filename = "V:\\Movie 2\\Movie 2.mkv"},
            };

            var result = digitalCopySync.GetAllFilesFromStorage("V:", "*.mkv");
            var fileInfos = new List<IFileInfo>
            {
                new MockFileInfo(fileSystem, @"V:\Movie 2\Movie 2.mkv"),
                new MockFileInfo(fileSystem, @"V:\Movie 1\Movie 1.mkv"),
                new MockFileInfo(fileSystem, @"V:\Movie 4\Movie 4.mkv")
            };

            var actual = digitalCopySync.ExcludeFileNamesAlreadyInUse(fileInfos, entriesWithFilename);
            Assert.Single(actual);
            Assert.Contains(@"V:\Movie 4\Movie 4.mkv", result.Select(i => i.FullName));

        }

        [Fact(Skip ="DbContext not injected")]
        public void FindMatchingTitles()
        {
            var entriesWithoutFilename = new List<videodb_videodata>
            {
                new videodb_videodata { id = 1, title = "Movie 1" },
                new videodb_videodata { id = 2, title = "Movie 2"},
                new videodb_videodata { id = 3, title = "Movie 3"},
                new videodb_videodata { id = 6, title = "Movie 4"},
            };

            var entriesWithFilename = new List<videodb_videodata>
            {
                new videodb_videodata { id = 4, title = "Movie 1", filename = "V:\\Movie 1\\Movie 1.mkv"},
                new videodb_videodata { id = 5, title = "Movie 2", filename = "V:\\Movie 2\\Movie 2.mkv"},
            };

            var result = digitalCopySync.GetAllFilesFromStorage("V:", "*.mkv");
            var fileInfos = new List<IFileInfo>
            {
                new MockFileInfo(fileSystem, @"V:\Movie 2\Movie 2.mkv"),
                new MockFileInfo(fileSystem, @"V:\Movie 1\Movie 1.mkv"),
                new MockFileInfo(fileSystem, @"V:\Movie 4\Movie 4.mkv")
            };

            var actual = digitalCopySync.FindMatchingTitles("V:", "*.mkv");
            Assert.Single(actual);
            Assert.Contains(@"V:\Movie 4\Movie 4.mkv", result.Select(i => i.FullName));

        }

        [Fact]
        public void GetAllFilesFromStorage()
        {
            var result = digitalCopySync.GetAllFilesFromStorage("V:", "*.mkv");
            Assert.Equal(3, result.Count());
            Assert.Contains(@"V:\Movie 2\Movie 2.mkv", result.Select(i => i.FullName));
        }
    }
}
