using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.Data.Context;
using Jaxx.VideoDb.Data.DatabaseModels;
using Jaxx.WebApi.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
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
        private readonly IFileSystem fileSystem;

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
            public IEnumerable<IFileInfo> matchingFiles;
        }

        public DigitalCopySync(ILogger<DigitalCopySync> logger, VideoDbContext context, MovieDataServiceOptions options, IFileSystem fileSystem)
        {
            this.logger = logger;
            this.context = context;
            this.options = options;
            this.fileSystem = fileSystem;
        }

        public DigitalCopySync(ILogger<DigitalCopySync> logger, VideoDbContext context, MovieDataServiceOptions options)
        {
            this.logger = logger;
            this.context = context;
            this.options = options;
            this.fileSystem = new FileSystem();
        }


        /// <summary>
        /// Get all entries from DB that have a filename set
        /// </summary>
        internal IEnumerable<videodb_videodata> GetDbEntriesWithFilename()
        {
            var result = context.VideoData.Where(item => !string.IsNullOrWhiteSpace(item.filename) && item.owner_id != options.DeletedUserId).ToList();
            logger.LogDebug("Found {0} db entries with a filename.", result.Count);
            return result;
        }

        /// <summary>
        /// Get all entries from DB that have no filename set
        /// </summary>
        /// <returns></returns>
        public IEnumerable<videodb_videodata> GetDbEntriesWithoutFileName()
        {
            var result = context.VideoData.Include(m => m.VideoMediaType).Where(item => string.IsNullOrWhiteSpace(item.filename) && item.owner_id != options.DeletedUserId).ToList();
            logger.LogDebug("Found {0} db entries without a filename.", result.Count);
            return result;
        }

        /// <summary>
        /// Get all files from the given path and the filter.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        internal IEnumerable<IFileInfo> GetAllFilesFromStorage(string path, string filter)
        {
            var fileinfolist = new List<IFileInfo>();
            var files = fileSystem.Directory.EnumerateFiles(path, filter, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                fileinfolist.Add(fileSystem.FileInfo.FromFileName(file));
            }
            return fileinfolist;
        }

        /// <summary>
        /// Gets a grouped list for db entries, which have the same filename and therefore
        /// the filename is given for multiple movies at the same time
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IGrouping<string, videodb_videodata>> GetDbEntriesWithSameFileName()
        {

            var resultList = new List<IGrouping<string, videodb_videodata>>();
            var groupedResult = GetDbEntriesWithFilename().GroupBy(item => item.filename);
            foreach (var group in groupedResult)
            {
                if (group.Count() > 1) resultList.Add(group);
            }

            logger.LogDebug("Found {0} filenames which are set multiple times.", resultList.Count);
            return resultList;
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
        public void ClearFilenameForNotExistingFiles(BlockingCollection<videodb_videodata> entriesWhereFileNotExists)
        {
            foreach (var notExistingFile in entriesWhereFileNotExists)
            {
                logger.LogWarning($"Deleting filename for movie with id '{notExistingFile.id}' and title '{notExistingFile.title}', filepath was '{notExistingFile.filename}'");
                context.VideoData.Where(item => item.id == notExistingFile.id).FirstOrDefault().filename = string.Empty;
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Check for each db entry without a filename if there is a match
        /// with a file from storage
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        /// <returns>Returns a list with all movies having at least one match including a list of matching files. Could be more than one.</returns>
        public IEnumerable<TitleMatch> FindMatchingTitles(string path, string filter)
        {
            logger.LogInformation("Find matching files");
            var matchList = new BlockingCollection<TitleMatch>();
            var files = GetAllFilesFromStorage(path, filter);
            var dbEntriesWithoutFilename = GetDbEntriesWithoutFileName();
            var dbEntriesWithFilename = GetDbEntriesWithFilename();

            Parallel.ForEach(dbEntriesWithoutFilename, entry =>
            {
                var escapedEntryTitle = entry.title
                    .Replace(":", " -")
                    .Replace("&", "und")
                    .Replace("ß", "ss")
                    .Replace("!", string.Empty)
                    .Replace("?", string.Empty)
                    .Replace("'", string.Empty)
                    .ToLower();

                var escpatedEntrySubtitle = entry.subtitle?
                    .Replace(":", " -")
                    .Replace("&", "und")
                    .Replace("ß", "ss")
                    .Replace("!", string.Empty)
                    .Replace("?", string.Empty)
                    .Replace("'", string.Empty)
                    .ToLower();

                var matches = files.Where(file => file.Directory.Name.ToLower() == escapedEntryTitle || file.Directory.Name.ToLower() == $"{escapedEntryTitle} - {escpatedEntrySubtitle}");
                matches = ExcludeFileNamesAlreadyInUse(matches, dbEntriesWithFilename);
                matchList.Add(new TitleMatch { Movie = entry, matchingFiles = matches });

            });

            return matchList;
        }

        internal IEnumerable<IFileInfo> ExcludeFileNamesAlreadyInUse(IEnumerable<IFileInfo> fileInfos, IEnumerable<videodb_videodata> dbEntriesWithFiles)
        {

            var fileListWithExcludedFiles = new BlockingCollection<IFileInfo>();

            Parallel.ForEach(fileInfos, file =>
            {
                var dbEntriesWithCurrentFileName = dbEntriesWithFiles.Where(i => i.filename.Contains(file.FullName));
                if (dbEntriesWithCurrentFileName.Count() > 0)
                {
                    logger.LogWarning("Filename '{0}' already in use for movie '{1}', dropping filename from list.", file.FullName, dbEntriesWithCurrentFileName.FirstOrDefault().title);
                } else
                {
                    logger.LogDebug("Add filename '{0}' to list", file.FullName);
                    fileListWithExcludedFiles.Add(file);
                }
            });
            return fileListWithExcludedFiles;
        }

        /// <summary>
        /// Updates the db filename of the given movies, if a unique file match is available.
        /// Otherwhise creates a log warning.
        /// </summary>
        /// <param name="updateList"></param>
        public void UpdateDbFilenames(IEnumerable<TitleMatch> updateList)
        {
            foreach (var entry in updateList)
            {
                if (entry.matchingFiles.Count() == 1)
                {
                    logger.LogInformation("Updating filename for title '{0}'", entry.Movie.title);
                    var filename = $"\"{entry.matchingFiles.FirstOrDefault().FullName}\"";
                    context.VideoData.Where(item => item.id == entry.Movie.id).FirstOrDefault().filename = filename;
                    context.SaveChanges();
                }
                else if (entry.matchingFiles.Count() > 1) logger.LogWarning("Can't update filename for title '{0}' because multiple files matches were found.", entry.Movie.title);
            }
        }

        /// <summary>
        /// Checks for each storage file if a db entry exisits.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        /// <returns>A list of files which have no match at db.</returns>
        public IEnumerable<IFileInfo> FindFilesWithoutDbEntries(string path, string filter)
        {
            var files = GetAllFilesFromStorage(path, filter);
            var dbEntriesWithFilename = GetDbEntriesWithFilename();
            var filesWithoutDbEntry = new BlockingCollection<IFileInfo>();

            Parallel.ForEach(files, file =>
            {
                var matches = dbEntriesWithFilename.Where(item => item.filename.Replace("\"", string.Empty) == file.FullName);
                if (!matches.Any()) filesWithoutDbEntry.Add(file);
            });

            return filesWithoutDbEntry;
        }
    }
}
