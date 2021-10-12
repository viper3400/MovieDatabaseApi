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
    public class FindMatchesWorker : IHostedService
    {
        private readonly IHostApplicationLifetime hostLifetime;
        private readonly ILogger<FindMatchesWorker> logger;
        private readonly DigitalCopySync digitalCopySync;
        private readonly FindMatchesOptions options;
        private int? exitCode;

        public FindMatchesWorker(IHostApplicationLifetime hostLifetime, ILogger<FindMatchesWorker> logger, DigitalCopySync digitalCopySync, FindMatchesOptions options)
        {
            this.hostLifetime = hostLifetime;
            this.logger = logger;
            this.digitalCopySync = digitalCopySync;
            this.options = options;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var result = digitalCopySync.FindMatchingTitles(options.Path, options.Filter);

            logger.LogInformation("Delete ouput file at '{0}'", options.Output);
            File.Delete(options.Output);

            logger.LogInformation("Iterate over {0} results", result.Count());
            foreach (var entry in result)
            {
                var title = entry.Movie.title;
                var fileMatchCount = entry.matchingFiles.Count();
                var matches = string.Join(",", entry.matchingFiles?.Select(item => item.FullName));

                File.AppendAllText(options.Output, $"{fileMatchCount};{title};{matches}{Environment.NewLine}");
            }
            logger.LogInformation("Created output file at '{0}'", options.Output);

            if (options.Update)
            {
                logger.LogInformation("Updating db entries if a match was found.");
                digitalCopySync.UpdateDbFilenames(result);
            }
            else logger.LogWarning("Skip updating db entries if a match was found. Use -u (--update) to do so.");

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
