using Jaxx.VideoDb.WebCore.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MovieDatabaseCLI
{
    public class CheckFileExistsWorker : IHostedService
    {
        private readonly IHostApplicationLifetime hostLifetime;
        private readonly ILogger<CheckFileExistsWorker> logger;
        private readonly DigitalCopySync digitalCopySync;
        private readonly CheckFilesExistsOptions options;
        private int? exitCode;

        public CheckFileExistsWorker(IHostApplicationLifetime hostLifetime, ILogger<CheckFileExistsWorker> logger, DigitalCopySync digitalCopySync, CheckFilesExistsOptions options)
        {
            this.hostLifetime = hostLifetime;
            this.logger = logger;
            this.digitalCopySync = digitalCopySync;
            this.options = options;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var result = digitalCopySync.CheckFilesOnStorage();

            File.Delete(options.ExistingFilesOutput);
            File.Delete(options.MissingFilesOutput);
            
            Parallel.Invoke(
                () => File.WriteAllLines(options.ExistingFilesOutput, result.EntriesWhereFileExists.Select(item => $"{item.title};{item.filename}")),
                () => File.WriteAllLines(options.MissingFilesOutput, result.EntriesWhereFileNotExists.Select(item => $"{item.title};{item.filename}")));

            if (options.Clear)
            {
                logger.LogInformation("Clear filename field on db for entries without existing file.");
                digitalCopySync.ClearFilenameForNotExistingFiles(result.EntriesWhereFileNotExists);
            } else logger.LogInformation("Skip function to clear filename field on db for entries without existing file. Use the -c (--clear) options to do so.");

            exitCode = 1;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Environment.ExitCode = exitCode.GetValueOrDefault(-1);
            logger.LogInformation($"Shutting down the service with code {Environment.ExitCode}");
            return Task.CompletedTask;
        }
    }
}
