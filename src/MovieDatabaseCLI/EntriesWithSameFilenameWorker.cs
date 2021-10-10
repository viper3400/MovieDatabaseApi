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
    public class EntriesWithSameFilenameWorker : IHostedService
    {
        private readonly IHostApplicationLifetime hostLifetime;
        private readonly ILogger<EntriesWithSameFilenameWorker> logger;
        private readonly DigitalCopySync digitalCopySync;
        private readonly EntriesWithSameFilenameOptions options;
        private int? exitCode;

        public EntriesWithSameFilenameWorker(IHostApplicationLifetime hostLifetime, ILogger<EntriesWithSameFilenameWorker> logger, DigitalCopySync digitalCopySync, EntriesWithSameFilenameOptions options)
        {
            this.hostLifetime = hostLifetime;
            this.logger = logger;
            this.digitalCopySync = digitalCopySync;
            this.options = options;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = digitalCopySync.GetDbEntriesWithSameFileName();

                logger.LogInformation("Delete ouput file at '{0}'", options.Output);
                File.Delete(options.Output);

                logger.LogInformation("Iterate over {0} results", result.Count());
                foreach (var entry in result)
                {
                    File.AppendAllText(options.Output, $"{entry.Key}{Environment.NewLine}");
                    foreach (var groupEntry in entry)
                    {
                        File.AppendAllText(options.Output, $"    |-{groupEntry.title} - {groupEntry.diskid}{Environment.NewLine}");
                    }
                }
                logger.LogInformation("Created ouput file at '{0}'", options.Output);

                exitCode = 1;
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                exitCode = -1;
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
