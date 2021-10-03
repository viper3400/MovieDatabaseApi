using Jaxx.VideoDb.Data.Context;
using Jaxx.VideoDb.Data.DatabaseModels;
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
    public class Worker : IHostedService
    {
        //private readonly DigitalCopySync digitalCopySync;
        private readonly string path;
        private readonly string filter;
        private readonly IHostApplicationLifetime _hostLifetime;
        private int? _exitCode;
        private readonly ILogger<Worker> _logger;
        private readonly VideoDbContext _context;
        public Worker ( IHostApplicationLifetime hostLifetime, ILogger<Worker> logger, VideoDbContext context)
        {
            _hostLifetime = hostLifetime ?? throw new ArgumentNullException(nameof(hostLifetime));

            //this.digitalCopySync = digitalCopySync;
            this.path = "V:\\Filme";
            this.filter = "*.mkv*";
            _context = context;
            _logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger?.LogInformation("Start the service.");

            try
            {
                var dataFromDatabase = new List<SyncResultModel>();
                var fileList = new List<SyncResultModel>();

                Parallel.Invoke(() => dataFromDatabase = GetDataFromDatabase().ToList(), () => fileList = GetFilesFromStorage().ToList());

                var entriesWithOutFileName = dataFromDatabase.Where(item => !item.FileExistsOnStorage);
                _logger.LogInformation("Entries without filename: {0}.", entriesWithOutFileName.Count());
                _logger.LogInformation("Entries in file list: {0}.", fileList.Count());

                var updateCandidates = new List<SyncResultModel>();
                var multipleResultsList = new List<SyncResultModel>();
                var noResultsList = new List<SyncResultModel>();

                Parallel.ForEach(entriesWithOutFileName, entry =>
                {
                    var movies = fileList.Where(file => file.Title == entry.Title || file.Title == $"{entry.Title} {entry.Subtitle}");
                    if (movies != null)
                    {
                        if (movies.Count() == 1)
                        {
                            //_logger.LogInformation("Update candidata: {0}.", movies.FirstOrDefault().Title);
                            updateCandidates.Add(new SyncResultModel { Title = entry.Title, FilePath = movies.FirstOrDefault().FilePath, Deleted = entry.Deleted });
                        }
                        else if (movies.Count() > 1)
                        {
                            multipleResultsList.Add(entry);
                        }
                        else noResultsList.Add(new SyncResultModel { Title = entry.Title, Deleted = entry.Deleted });
                    }
                });

                Parallel.Invoke(
                    () => WriteToFile(updateCandidates, "D:\\updatecandidates.txt"),
                    () => WriteToFile(multipleResultsList, "D:\\multiResults.txt"),
                    () => WriteToFile(noResultsList, "D:\\noResults.txt"));

                _logger.LogInformation("Update candidates: {0}.", updateCandidates.Count());
                _logger.LogInformation("Multiple results: {0}.", multipleResultsList.Count());
                _logger.LogInformation("No results: {0}.", noResultsList.Count());
            }
            finally
            {
                _hostLifetime.StopApplication();
            }
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Environment.ExitCode = _exitCode.GetValueOrDefault(-1);
            _logger?.LogInformation($"Shutting down the service with code {Environment.ExitCode}");
            return Task.CompletedTask;
        }

        private IEnumerable<SyncResultModel> GetDataFromDatabase()
        {
            _logger?.LogInformation("Start catching movieds from database.");
            var result = _context.VideoData;
            var resultList = new List<SyncResultModel>();
            foreach (var entry in result)
            {
                var fileName = string.IsNullOrWhiteSpace(entry.filename) ? string.Empty : entry.filename.Replace("\"", string.Empty);
                var isExisting = File.Exists(fileName);
                
                resultList.Add(new SyncResultModel { Title = entry.title, Subtitle = entry.subtitle, FilePath = fileName, FileExistsOnStorage = isExisting, Deleted = entry.owner_id});

                if (!isExisting)
                {
                    File.AppendAllText("D:\\result_X.txt", $"{entry.title};{fileName};{isExisting}{Environment.NewLine}");
                }
            }

            _logger?.LogInformation("Finished catching  movies from database.");
            return resultList;
        }

        private IEnumerable<SyncResultModel> GetFilesFromStorage()
        {
            _logger?.LogInformation("Start enumerating movie files.");
            var fileResultList = new List<SyncResultModel>();
            var fileList =  Directory.EnumerateFiles(this.path, this.filter, SearchOption.AllDirectories);
            foreach (var file in fileList)
            {
                var fi = new FileInfo(file);
                fileResultList.Add(new SyncResultModel { Title = fi.Directory.Name,  FilePath = file, FileExistsOnStorage = true });
            }

            _logger?.LogInformation("Finished enumerating movie files.");
            return fileResultList;
        }

        private void WriteToFile(List<SyncResultModel> list, string path)
        {
            foreach(var entry in list)
            {
                File.AppendAllText(path, $"{entry.Deleted};{entry.Title};{entry.FilePath}{Environment.NewLine}");
            }
        }
    }
}
