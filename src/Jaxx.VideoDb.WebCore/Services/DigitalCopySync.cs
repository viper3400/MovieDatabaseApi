using Jaxx.WebApi.Shared.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Services
{
    public class DigitalCopySync
    {
        private readonly ILogger<DefaultMovieDataService> logger;
        private readonly IMovieDataService movieDataService;

        public DigitalCopySync(ILogger<DefaultMovieDataService> logger, IMovieDataService movieDataService)
        {
            this.logger = logger;
            this.movieDataService = movieDataService;
        }

        public async Task<List<string>> ScanDigitalCopies(string directory, string filter)
        {
            var resultList = new List<string>();
            var files = Directory.EnumerateFiles(directory, filter, SearchOption.AllDirectories);

            // --> Can't access the db in parallel the way MovieDataService is build
            //Parallel.ForEach(files, file =>
            //{
            //    var result = CheckDigitalCopyEntryForPath(file);
            //    if (!string.IsNullOrWhiteSpace(result.Result)) resultList.Add(result.Result);
            //});

            foreach (var file in files)
            {
                var result = await CheckDigitalCopyEntryForPath(file);
                if (!string.IsNullOrWhiteSpace(result)) resultList.Add(result);
            }
            return resultList;
        }

        private async Task<string> CheckDigitalCopyEntryForPath(string path)
        {
            string result;
            var fi = new FileInfo(path);
            var dbResult = await movieDataService.GetMovieDataAsync(null, new PagingOptions { Limit = 2, Offset = 0 }, new Models.MovieDataOptions { Title = fi.Directory.Name}, new System.Threading.CancellationToken());
            if (dbResult.TotalSize < 1) return $"'{fi.Directory.Name}';'No DB entry found for this file.';'${fi.FullName}'";
            if (dbResult.TotalSize > 1) result = $"'{fi.Directory.Name}';'Got more then one result for this file.';'${fi.FullName}'";
            else
            {
                var dBFilename = dbResult.Items.FirstOrDefault().filename != null ? dbResult.Items.FirstOrDefault().filename : "";
                if (string.IsNullOrWhiteSpace(dBFilename)) result = $"'{fi.Directory.Name}';'This is a match, but filepath is not set on db.'";
                else if (dBFilename.Replace("\"", "").ToLower() != path.ToLower()) result = $"'{fi.Directory.Name}';'This is a match, but db entry is not correct '{dBFilename}';'${fi.FullName}'";
                else result = "";
            }

            return result;
        }
    }
}
