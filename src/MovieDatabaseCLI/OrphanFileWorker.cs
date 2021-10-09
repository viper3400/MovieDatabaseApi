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
    public class OrphanFileWorker : IHostedService
    {
        private readonly IHostApplicationLifetime hostLifetime;
        private readonly ILogger<OrphanFileWorker> logger;
        private readonly DigitalCopySync digitalCopySync;
        private readonly OrphanFilesOptions options;
        private int? exitCode;

        public OrphanFileWorker(IHostApplicationLifetime hostLifetime, ILogger<OrphanFileWorker> logger, DigitalCopySync digitalCopySync, OrphanFilesOptions options)
        {
            this.hostLifetime = hostLifetime;
            this.logger = logger;
            this.digitalCopySync = digitalCopySync;
            this.options = options;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Start the service.");

            try
            {
                var orphanedFiles = digitalCopySync.FindFilesWithoutDbEntries(options.Path, options.Filter);

                var fileContent = new List<string>();
                foreach (var file in orphanedFiles)
                {
                    fileContent.Add($"{file.FullName}");
                }
                
                if (File.Exists(options.Output)) File.Delete(options.Output);
                
                File.AppendAllLines(options.Output, orphanedFiles.Select(file => file.FullName).ToList());
            }
            finally
            {
                exitCode = 1;
                hostLifetime.StopApplication();
            }

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
