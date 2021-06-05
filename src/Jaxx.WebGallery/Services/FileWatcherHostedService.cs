using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaxx.WebGallery.Services
{
    public class FileWatcherHostedService : BackgroundService
    {
        private readonly ILogger logger;
        private readonly GalleryConfiguration config;
        private readonly IGalleryService galleryService;

        public FileWatcherHostedService(ILogger<FileWatcherHostedService> logger, GalleryConfiguration config, IGalleryService galleryService)
        {
            this.logger = logger;
            this.config = config;
            this.galleryService = galleryService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation($"Started {nameof(FileWatcherHostedService)}.");
            galleryService.CheckDbConsistency();
            var watcher = Run(config.ImagePath); ;

            while (!stoppingToken.IsCancellationRequested)
            {
                await Watch();
            }

            logger.LogInformation($"{nameof(FileWatcherHostedService)} shutting down.");
            watcher.Dispose();
        }

        private async Task Watch()
        {

            await Task.Run(() =>
            {
                logger.LogDebug($"{nameof(FileWatcherHostedService)} is alive.");
                Thread.Sleep(60 * 60 * 1000);
            });
        }

        //[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private FileSystemWatcher Run(string path)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();

            watcher.IncludeSubdirectories = true;
            watcher.Path = path;
            logger.LogInformation($"Watching path {path}.");

            // Watch for changes in LastAccess and LastWrite times, and
            // the renaming of files or directories.
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;

                // Only watch text files.
                // watcher.Filter = "*.txt";

                // Add event handlers.
                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                watcher.Deleted += OnChanged;
                watcher.Renamed += OnRenamed;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

            return watcher;
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            switch(e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                case WatcherChangeTypes.Created:
                    FileAttributes attr = File.GetAttributes(e.FullPath);
                    logger.LogDebug($"(1) File: {e.FullPath} {e.ChangeType}, attribute: {attr}");
                    break;
                case WatcherChangeTypes.Deleted:
                    logger.LogDebug($"(2) File: {e.FullPath} {e.ChangeType}");
                    break;
                default:
                    logger.LogDebug($"(3) File: {e.FullPath} {e.ChangeType}");
                    break;
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e) =>
            // Specify what is done when a file is renamed.
            logger.LogDebug($"File: {e.OldFullPath} renamed to {e.FullPath}");

    }

}
