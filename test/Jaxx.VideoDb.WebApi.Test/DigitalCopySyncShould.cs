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
    [Collection("AutoMapperCollection")]
    public class DigitalCopySyncShould
    {
        private readonly DigitalCopySync digitalCopySync;
        private readonly ITestOutputHelper testOutputHelper;
        private readonly ILogger logger;

        public DigitalCopySyncShould(ITestOutputHelper testOutputHelper)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new XunitLoggerProvider(testOutputHelper));
            logger = loggerFactory.CreateLogger<DigitalCopySync>();

            var host = TestMovieDataServiceHost.Host().Build();
            host.StartAsync().Wait();
            digitalCopySync = host.Services.GetService(typeof(DigitalCopySync)) as DigitalCopySync;
        }

        [Fact]
        [Trait("Category", "Online")]
        public void GetAllEntriesWithAFileNameSet()
        {
            var result = digitalCopySync.GetDbEntriesWithFilename();
            Assert.Equal("\"V:\\Filme\\Was nützt die Liebe in Gedanken\\Was nützt die Liebe in Gedanken.mkv\"", result.Where(item => item.title == "Was nützt die Liebe in Gedanken").FirstOrDefault()?.filename);
            Assert.Equal(2307, result.Count());
        }

        [Fact]
        [Trait("Category", "Online")]
         public void GetExistingFileLists()
        {
            var result = digitalCopySync.CheckFilesOnStorage();
            Assert.Equal(2307, result.EntriesAll.Count);
            Assert.Empty(result.EntriesWhereFileNotExists);
            Assert.Equal(2307, result.EntriesWhereFileExists.Count);
        }

        [Fact]
        [Trait("Category", "Online")]
        public void ClearFilenameForNotExistingFiles()
        {
            var existingFilesList = digitalCopySync.CheckFilesOnStorage();
            digitalCopySync.ClearFilenameForNotExistingFiles(existingFilesList.EntriesWhereFileNotExists);
        }

    }
}
