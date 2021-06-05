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
            if (dbResult.TotalSize < 1) return $"Got no result for title ''{fi.Directory.Name}'.";
            if (dbResult.TotalSize > 1) result = $"Got more then one result for title '{fi.Directory.Name}'.";
            else
            {
                var dBFilename = dbResult.Items.FirstOrDefault().filename != null ? dbResult.Items.FirstOrDefault().filename : "";
                if (string.IsNullOrWhiteSpace(dBFilename)) result = $"No file found for title '{fi.Directory.Name}'. Would do update here!";
                else if (dBFilename.Replace("\"","").ToLower() != path.ToLower()) result = $"DigitalCopyPath: {path} - Mismatch with db path: {dBFilename}";
                else result = "";
            }

            return result;
        }
    }
}
