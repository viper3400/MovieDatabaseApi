using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.Data.Context;
using Jaxx.VideoDb.Data.DatabaseModels;
using Jaxx.WebApi.Shared.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Services
{
    public class DigitalCopySync
    {
        private readonly ILogger<DigitalCopySync> logger;
        private readonly VideoDbContext context;
        private readonly MovieDataServiceOptions options;

        /// <summary>
        /// Holds three BlockingCollection<videodb_videodata>
        /// </summary>
        public struct FileCheckResult
        {
            public BlockingCollection<videodb_videodata> EntriesWhereFileNotExists;
            public BlockingCollection<videodb_videodata> EntriesWhereFileExists;
            public BlockingCollection<videodb_videodata> EntriesAll;
        }

        public struct TitleMatch
        {
            public videodb_videodata Movie;
            public IEnumerable<FileInfo> matchingFiles;
        }
        public DigitalCopySync(ILogger<DigitalCopySync> logger, VideoDbContext context, MovieDataServiceOptions options)
        {
            this.logger = logger;
            this.context = context;
            this.options = options;
        }

        /// <summary>
        /// Get all entries from DB that have a filename set
        /// </summary>
        internal IEnumerable<videodb_videodata> GetDbEntriesWithFilename()
        {
            return context.VideoData.Where(item => !string.IsNullOrWhiteSpace(item.filename) && item.owner_id != options.DeletedUserId).ToList();
        }

        /// <summary>
        /// Get all entries from DB that have no filename set
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<videodb_videodata> GetDbEntriesWithoutFileName()
        {
            return context.VideoData.Where(item => string.IsNullOrWhiteSpace(item.filename) && item.owner_id != options.DeletedUserId).ToList();
        }

        /// <summary>
        /// Get all files from the given path and the filter.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        internal static IEnumerable<FileInfo> GetAllFilesFromStorage(string path, string filter)
        {
            var fileInfoList = new List<FileInfo>();
            var files = Directory.EnumerateFiles(path, filter, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                fileInfoList.Add(new FileInfo(file));
            }
            return fileInfoList;
        }

        /// <summary>
        /// Processes all entries from DB that have a file name set. Checks foreach of those
        /// entries, if a the file still exists, which is set in the field.
        /// </summary>
        /// <returns>Two lists, one holding all the entries for which the file exists, one holding
        /// all the entries, where no fiel exists</returns>
        public FileCheckResult CheckFilesOnStorage()
        {

            var resultLists = new FileCheckResult();
            resultLists.EntriesWhereFileExists = new BlockingCollection<videodb_videodata>();
            resultLists.EntriesWhereFileNotExists = new BlockingCollection<videodb_videodata>();
            resultLists.EntriesAll = new BlockingCollection<videodb_videodata>();

            var movieList = GetDbEntriesWithFilename();

            Parallel.ForEach(movieList, movie =>
            {
                resultLists.EntriesAll.Add(movie);

                if (File.Exists(movie.filename.Replace("\"", string.Empty)))
                    resultLists.EntriesWhereFileExists.Add(movie);
                else
                    resultLists.EntriesWhereFileNotExists.Add(movie);
               
            });
            return resultLists;
        }

        /// <summary>
        /// Removes the filename from the db entries given in the list.
        /// </summary>
        /// <param name="entriesWhereFileNotExists"></param>
        /// <param name="dryMode"></param>
        public void ClearFilenameForNotExistingFiles(BlockingCollection<videodb_videodata> entriesWhereFileNotExists, bool dryMode = true)
        {

            foreach (var notExistingFile in entriesWhereFileNotExists)
            {
                if (!dryMode)
                {
                    logger.LogWarning($"Deleting filename for movie with id '{notExistingFile.id}' and title '{notExistingFile.title}', filepath was '{notExistingFile.filename}'");
                    context.VideoData.Where(item => item.id == notExistingFile.id).FirstOrDefault().filename = string.Empty;
                    context.SaveChanges();
                }
                else logger.LogWarning($"DRYMODE: Deleting filename for movie with id '{notExistingFile.id}' and title '{notExistingFile.title}', filepath was '{notExistingFile.filename}'");
             }
        }

        internal IEnumerable<TitleMatch> FindMatchingTitles(string path, string filter)
        {
            logger.LogInformation("Find matching files");
            var matchList = new BlockingCollection<TitleMatch>();
            var files = GetAllFilesFromStorage(path, filter);
            var dbEntriesWithoutFilename = GetDbEntriesWithoutFileName();

            Parallel.ForEach(dbEntriesWithoutFilename, entry =>
            {
                var matches = files.Where(file => file.Directory.Name.ToLower() == entry.title.ToLower() || file.Directory.Name.ToLower() == $"{entry.title.ToLower()} {entry.subtitle?.ToLower()}");
                matchList.Add(new TitleMatch { Movie = entry, matchingFiles = matches });

            });

            return matchList;
        }

        public void UpdateFilenames(IEnumerable<TitleMatch> updateList)
        {
            foreach (var entry in updateList)
            {
                if (entry.matchingFiles.Count() == 1)
                {
                    logger.LogInformation("Updating filename for title '{0}'", entry.Movie.title);
                    var filename = $"\"{entry.matchingFiles.FirstOrDefault().FullName}\"";
                    context.VideoData.Where(item => item.id == entry.Movie.id).FirstOrDefault().filename = filename;
                    context.SaveChanges();
                } logger.LogWarning("Can't update filename for title '{0}' because multiple files matches were found.", entry.Movie.title);
            }
        }

        public IEnumerable<FileInfo> FindFilesWithoDbEntries(string path, string filter)
        {
            var files = GetAllFilesFromStorage(path, filter);
            var dbEntriesWithFilename = GetDbEntriesWithFilename();
            var filesWithoutDbEntry = new BlockingCollection<FileInfo>();

            Parallel.ForEach(files, file =>
            {
                var matches = dbEntriesWithFilename.Where(item => item.filename.Replace("\"", string.Empty) == file.FullName);
                if (!matches.Any()) filesWithoutDbEntry.Add(file);
            });

            return filesWithoutDbEntry;
        }
    }
}
