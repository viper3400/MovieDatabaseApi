using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Jaxx.VideoDb.WebCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Jaxx.VideoDb.Data.Context;
using Jaxx.VideoDb.Data.DatabaseModels;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.WebCore.Infrastructure;
using Jaxx.VideoDb.Data.BusinessLogic;
using System.IO;
using Jaxx.Images;
using Jaxx.WebApi.Shared.Models;

namespace Jaxx.VideoDb.WebCore.Services
{
    public class DefaultMovieDataService : IMovieDataService
    {
        private readonly VideoDbContext _context;
        private readonly ILogger<DefaultMovieDataService> _logger;
        private readonly IMapper _mapper;
        private readonly MovieDataServiceOptions _options;
        private readonly IMovieImageDownloadService _imageDownloader;
        private readonly DiskIdGenerator _diskIdGenerator;
        private readonly ImageStreamer imageStreamer;

        public DefaultMovieDataService(VideoDbContext context,
            MovieDataServiceOptions serviceOptions,
            ILogger<DefaultMovieDataService> logger,
            IMapper mapper,
            IMovieImageDownloadService imageDownloader,
            DiskIdGenerator diskIdGenerator,
            ImageStreamer imageStreamer)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _options = serviceOptions;
            _imageDownloader = imageDownloader;
            _diskIdGenerator = diskIdGenerator;
            this.imageStreamer = imageStreamer;
            _logger.LogDebug("New instance created.");
        }

        /// <summary>
        /// Adds a new entry from MovieDataResource (without genres). LastUpdate and Created will be set to now.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<MovieDataResource> CreateMovieDataAsync(MovieDataResource item, CancellationToken ct)
        {
            var entity = _mapper.Map<videodb_videodata>(item);
            entity.created = DateTime.Now;
            entity.lastupdate = DateTime.Now;

            await _context.VideoData.AddAsync(entity, ct);
            await _context.SaveChangesAsync();

            _imageDownloader.DownloadCoverImageAsync(entity);
            await _imageDownloader.DownloadBackgroundImageAsync(entity);

            var createdEntity = await GetMovieDataAsync(entity.id, ct);
            var createdResoure = _mapper.Map<MovieDataResource>(createdEntity);
            return createdResoure;
        }

        public async Task<bool> DeleteMovieDataAsync(int id, CancellationToken cancellationToken)
        {
            var entity = await _context.VideoData.SingleOrDefaultAsync(v => v.id == id);
            _context.VideoData.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MovieDataResource> GetMovieDataAsync(int id, CancellationToken ct)
        {
            _logger.LogTrace("Get information for id {0}", id);

            var entity = await _context
              .VideoData
              .Include(o => o.VideoOwner)
              .Include(v => v.VideoGenres)
              .ThenInclude(g => g.Genre)
              .Include(m => m.VideoMediaType)
              .Include(s => s.SeenInformation)
              .Include(s => s.UserSettings)
              .SingleOrDefaultAsync(x => x.id == id, ct);

            return _mapper.Map<MovieDataResource>(entity, opt => opt.Items[Controllers.Infrastructure.AutoMapperConstants.INLINE_COVER_IMAGE] = true);
        }

        public async Task<Page<MovieDataResource>> GetMovieDataAsync(List<int> movieIds, PagingOptions pagingOptions, MovieDataOptions movieDataOptions, CancellationToken ct)
        {
            _logger.LogTrace("Searching ...");
            IQueryable<videodb_videodata> query = _context.VideoData;

            if (movieIds != null)
            {
                _logger.LogTrace("Add movie id {0} to query", string.Join(",", movieIds));
                query = query.Where(x => movieIds.Contains(x.id));
            }


            query = HandleMovieDataOptions(movieDataOptions, query);

            var size = await query.CountAsync(ct);

            var items = await query
                .Include(o => o.VideoOwner)
                .Include(v => v.VideoGenres)
                .ThenInclude(g => g.Genre)
                .Include(m => m.VideoMediaType)
                .Include(s => s.SeenInformation)
                .Include(s => s.UserSettings)
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ToListAsync(ct);

            var mappedItems = _mapper.Map<IEnumerable<MovieDataResource>>(items, opt => opt.Items[Controllers.Infrastructure.AutoMapperConstants.INLINE_COVER_IMAGE] = movieDataOptions.UseInlineCoverImage);

            return new Page<MovieDataResource>
            {
                Items = mappedItems,
                TotalSize = size
            };
        }

        private IQueryable<videodb_videodata> HandleMovieDataOptions(MovieDataOptions movieDataOptions, IQueryable<videodb_videodata> query)
        {
            if (!string.IsNullOrWhiteSpace(movieDataOptions.IsDeleted))
            {
                query = QueryFilterByDeletedOption(movieDataOptions.IsDeleted, query);
            }

            if (!string.IsNullOrWhiteSpace(movieDataOptions.NotSeen))
            {
                query = QueryFilterByNotSeen(movieDataOptions.NotSeen, query);
            }

            if (!string.IsNullOrWhiteSpace(movieDataOptions.Search))
            {
                if (Regex.IsMatch(movieDataOptions.Search, @"R\d{2}"))
                {
                    query = QueryFilterMovieByDiskid(movieDataOptions.Search, query);
                }
                else query = QueryFilterByMovieTitle(movieDataOptions.Search, movieDataOptions.ExactMatch, query);
            }

            if (!string.IsNullOrWhiteSpace(movieDataOptions.MediaTypes))
            {
                query = QueryFilterByMediaType(movieDataOptions.MediaTypes, query);
            }

            if (!string.IsNullOrWhiteSpace(movieDataOptions.Diskid))
            {
                query = QueryFilterMovieByDiskid(movieDataOptions.Diskid, query);
            }

            if (!string.IsNullOrWhiteSpace(movieDataOptions.Title))
            {
                query = QueryFilterByMovieTitle(movieDataOptions.Title, movieDataOptions.ExactMatch, query);
            }

            if (!string.IsNullOrWhiteSpace(movieDataOptions.Genres))
            {
                query = QueryFilterByMovieGenres(movieDataOptions.Genres, query);
            }

            if (!string.IsNullOrWhiteSpace(movieDataOptions.IsTv))
            {
                query = QueryFilterByTvOption(movieDataOptions.IsTv, query);
            }

            // Apply sort order
            query = QuerySortOrder(movieDataOptions.SortOrder, query);

            return query;
        }

        private IQueryable<videodb_videodata> QuerySortOrder(MovieDataSortOrder sortOrder, IQueryable<videodb_videodata> query)
        {
            switch (sortOrder)
            {
                case MovieDataSortOrder.ByLastSeenDateAsc:
                    query = query
                        .OrderBy(i => i.SeenInformation.OrderByDescending(s => s.viewdate).FirstOrDefault().viewdate);
                    break;
                default:
                case MovieDataSortOrder.ByDiskIdAsc:
                    query = query.OrderBy(i => i.diskid);
                    break;
            }

            return query;
        }

        private IQueryable<videodb_videodata> QueryFilterByNotSeen(string notSeen, IQueryable<videodb_videodata> query)
        {
            _logger.LogTrace("Filter for NotSeen: {0}", notSeen);

            TimeSpan days;
            if (TimeSpan.TryParse(notSeen, out days))
            {
                var notSeenBeforeDate = DateTime.Now - days;
                _logger.LogTrace("Filter for movies not seen since {0}", notSeenBeforeDate.ToShortDateString());
                var movieIds = new List<int>(_context.HomeWebUserSeen.Where(s => s.viewdate >= notSeenBeforeDate).Select(s => s.vdb_videoid).ToList());
                _logger.LogTrace($"Count {movieIds.Count()}");

                query = query.Where(v => !(movieIds.Contains(v.id)));
            }
            else _logger.LogError("Could not parse value to TimeSpan");

            return query;
        }

        private IQueryable<videodb_videodata> QueryFilterByDeletedOption(string isDeleted, IQueryable<videodb_videodata> query)
        {
            _logger.LogTrace("Filter for isDeleted = {0}", isDeleted);

            if (bool.Parse(isDeleted))
            {
                query = query.Where(v => v.owner_id == _options.DeletedUserId);
            }
            else query = query.Where(v => v.owner_id != _options.DeletedUserId);

            return query;
        }

        private IQueryable<videodb_videodata> QueryFilterByTvOption(string isTv, IQueryable<videodb_videodata> query)
        {
            _logger.LogTrace("Filter for isTv = {0}", isTv);
            query = query.Where(v => v.istv == bool.Parse(isTv));
            return query;
        }

        private IQueryable<videodb_videodata> QueryFilterByMovieGenres(string genres, IQueryable<videodb_videodata> query)
        {
            _logger.LogTrace("Filter for genres: {0}", genres);
            var genreArray = genres.Split(',').ToList();

            var genreIdList = _context.Genres.ToList();
            var genreIds = genreIdList.Where(g => genreArray.Contains(g.name, StringComparer.OrdinalIgnoreCase)).Select(g => g.id);

            // just go on if there are any results (otherwise the resultList will be kept empty)
            if (genreIds.Any())
            {
                var movieIdsForSelectedGenre = from genre in _context.Genre
                                               where genreIds.Contains(genre.genre_id)
                                               select genre.video_id;

                // group by movieids and return just these where all genres are present
                var movieIds = movieIdsForSelectedGenre.GroupBy(m => m).Where(grp => grp.Count() == genreIds.Count()).Select(m => m.Key).ToList();

                query = query.Where(m => movieIds.Contains(m.id));
            }
            else
            {
                _logger.LogError($"Invalid genres: {genres}");
                // emtpy result (1 never will be 2)
                query = query.Where(m => 1 == 2);
            }
            return query;
        }

        private IQueryable<videodb_videodata> QueryFilterByMediaType(string mediaTypes, IQueryable<videodb_videodata> query)
        {
            _logger.LogTrace("Filter for media type id {0}.", mediaTypes);
            var mediaTypesArray = mediaTypes.Split(',').ToList();
            if (mediaTypesArray.Any())
            {
                query = query.Where(m => mediaTypesArray.Contains(m.mediatype.ToString()));
            }

            return query;
        }

        private IQueryable<videodb_videodata> QueryFilterByMovieTitle(string title, bool exactMatch, IQueryable<videodb_videodata> query)
        {
            _logger.LogTrace("Searching for title: {0}, exactMatch: {1}", title, exactMatch);
            if (!exactMatch)
            {
                query = query.Where(v => v.title.Contains(title.ToLower()) || v.subtitle.Contains(title.ToLower()));
            } else query = query.Where(v => v.title.ToLower() == title.ToLower() || v.subtitle.ToLower() == title.ToLower());
            return query;
        }

        private IQueryable<videodb_videodata> QueryFilterMovieByDiskid(string diskId, IQueryable<videodb_videodata> query)
        {
            _logger.LogTrace("Searching for diskid: {0}", diskId);
            query = query.Where(v => v.diskid.StartsWith(diskId));
            return query;
        }

        public async Task<Page<MovieDataEnhancedResource>> GetMovieDataEnhancedAsync(int? movieId, PagingOptions pagingOptions, MovieDataOptions movieDataOptions, CancellationToken ct)
        {
            IQueryable<videodb_videodata> query = _context.VideoData;

            if (movieId != null)
            {
                query = query.Where(x => x.id == movieId);
            }

            if (!String.IsNullOrWhiteSpace(movieDataOptions.Diskid))
            {
                query = query.Where(v => v.diskid.StartsWith(movieDataOptions.Diskid));
            }

            if (!String.IsNullOrWhiteSpace(movieDataOptions.Title))
            {
                query = query.Where(v => v.title.Contains(movieDataOptions.Title) || v.subtitle.Contains(movieDataOptions.Title));
            }

            // todo apply search
            // todo apply sort

            var size = await query.CountAsync(ct);

            var items = await query
                .AsNoTracking()
                .OrderBy(i => i.diskid)
                //.Join(_context.Genre, movie => movie.id, moviegenres => moviegenres.video_id, ( movie, moviegenres) => new { movie, moviegenres })
                //.Join(_context.Genres, genreFroMovie => genreFroMovie.moviegenres.genre_id, genres => genres.id, ( genreFroMovie, genres) => new { genreFroMovie, genres })                
                //.GroupBy(g => g.genreFroMovie.movie.id)
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                //.Select(s => s.Key)
                .ProjectTo<MovieDataEnhancedResource>(_mapper.ConfigurationProvider)
                .ToArrayAsync(ct);


            var page = new Page<MovieDataEnhancedResource>
            {
                Items = items,
                TotalSize = size
            };

            return page;
        }

        /// <summary>
        /// Updates a movie with a resource. The resource need to be complete, because the original entity will
        /// be overwritten completely. The id, diskid and created field won't be touched by this operation.
        /// LastUpdated will be set to now.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updatedMovieResource"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MovieDataResource> UpdateMovieDataAsync(int id, MovieDataResource updatedMovieResource, CancellationToken cancellationToken)
        {

            var movieEntity = await _context.VideoData.SingleOrDefaultAsync(v => v.id == id);
            var updatedEntity = MapRessourceToEntity(updatedMovieResource, movieEntity);
            updatedEntity.lastupdate = DateTime.Now;

            await _context.SaveChangesAsync();

            _imageDownloader.DownloadCoverImageAsync(updatedEntity);
            await _imageDownloader.DownloadBackgroundImageAsync(updatedEntity);

            return _mapper.Map<MovieDataResource>(updatedEntity);
        }

        private videodb_videodata MapRessourceToEntity(MovieDataResource movieDataResource, videodb_videodata movieEntity)
        {
            movieEntity.title = movieDataResource.title;
            movieEntity.subtitle = movieDataResource.subtitle;
            movieEntity.actors = movieDataResource.actors;
            movieEntity.country = movieDataResource.country;
            movieEntity.custom1 = movieDataResource.custom1;
            movieEntity.custom2 = movieDataResource.custom2;
            movieEntity.custom3 = movieDataResource.custom3;
            movieEntity.custom4 = movieDataResource.custom4;
            movieEntity.audio_codec = movieDataResource.audio_codec;
            movieEntity.director = movieDataResource.director;
            movieEntity.disklabel = movieDataResource.disklabel;
            movieEntity.filedate = movieDataResource.filedate;
            movieEntity.filename = movieDataResource.filename;
            movieEntity.filesize = movieDataResource.filesize;
            movieEntity.imdbID = movieDataResource.imdbID;
            movieEntity.imgurl = movieDataResource.imgurl;
            movieEntity.istv = movieDataResource.istv;
            movieEntity.language = movieDataResource.language;
            movieEntity.md5 = movieDataResource.md5;
            movieEntity.mediatype = movieDataResource.mediatype;
            movieEntity.owner_id = movieDataResource.owner_id;
            movieEntity.plot = movieDataResource.plot;
            movieEntity.rating = movieDataResource.rating;
            movieEntity.runtime = movieDataResource.runtime;
            movieEntity.video_codec = movieDataResource.video_codec;
            movieEntity.video_height = movieDataResource.video_height;
            movieEntity.video_width = movieDataResource.video_height;
            movieEntity.year = movieDataResource.year;
            movieEntity.comment = movieDataResource.comment;
            movieEntity.diskid = movieDataResource.diskid;

            return movieEntity;
        }


        public async Task<Page<MovieDataSeenResource>> GetSeenMovies(PagingOptions pagingOptions, DateRangeFilterOptions dateRangeFilteroptions, CancellationToken cancellationToken)
        {
            IQueryable<homewebbridge_userseen> query = _context.HomeWebUserSeen;
            if (dateRangeFilteroptions.FromDate != null)
            {
                query = query.Where(d => d.viewdate >= dateRangeFilteroptions.FromDate);
            }
            if (dateRangeFilteroptions.ToDate != null)
            {
                query = query.Where(d => d.viewdate <= dateRangeFilteroptions.ToDate);
            }
            var size = await query.CountAsync(cancellationToken);
            var items = await query
                .AsNoTracking()
                .Include(s => s.MovieInformation)
                .ThenInclude(s => s.VideoGenres)
                .ThenInclude(s => s.Genre)
                .Include(s => s.MovieInformation)
                .ThenInclude(s => s.UserSettings)
                .OrderByDescending(d => d.viewdate)
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ToListAsync(cancellationToken);

            var mappedItems = _mapper.Map<IEnumerable<MovieDataSeenResource>>(items, opt => opt.Items[Controllers.Infrastructure.AutoMapperConstants.INLINE_COVER_IMAGE] = true);

            var page = new Page<MovieDataSeenResource>
            {
                Items = mappedItems,
                TotalSize = size
            };

            return page;
        }

        /// <summary>
        /// Set a movie with the given id as seen a viewgroup and a date. User id is need to log the username,
        /// but seen dates a based on view group, not on user level.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        /// <param name="viewGroup"></param>
        /// <param name="date"></param>
        /// <returns>The id of the generated seen entry. (This id is not the movie id!) </returns>
        public async Task<Tuple<int, string>> MovieSeenSetAsync(int id, string userName, string viewGroup, DateTime date)
        {
            Tuple<int, string> result;

            // Check for existing view entry
            var viewEntry = GetViewEntry(id, viewGroup, date.Date);

            if (viewEntry == null)
            {
                var entry = new homewebbridge_userseen
                {
                    asp_username = userName,
                    asp_viewgroup = viewGroup,
                    viewdate = date.Date,
                    vdb_videoid = id
                };

                _context.HomeWebUserSeen.Add(entry);
                await _context.SaveChangesAsync();

                result = new Tuple<int, string>(entry.id, $"Set movie with id {id} seen for {userName} in viewgroup {viewGroup} for date {date.ToShortDateString()}.");

            }
            else result = new Tuple<int, string>(-1, $"Entry for movie with id {id} for {userName} in viewgroup {viewGroup} for date {date.ToShortDateString()} allready exists.");
            return result;
        }

        /// <summary>
        /// Delete the given seen date for the given viewgroup and movieid. User id is need to log the username,
        /// but seen dates a based on view group, not on user level.
        /// </summary>
        /// <param name="id"></param>        
        /// <param name="viewGroup"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<Tuple<int, string>> MovieSeenDeleteAsync(int id, string viewGroup, DateTime date)
        {
            Tuple<int, string> result;

            var viewEntry = GetViewEntry(id, viewGroup, date.Date);

            if (viewEntry != null)
            {
                _context.HomeWebUserSeen.Remove(viewEntry);
                await _context.SaveChangesAsync();
                result = new Tuple<int, string>(1, $"Removed movie seen date for id {id} in viewgroup {viewGroup} for date {date.ToShortDateString()}.");
            }
            else result = new Tuple<int, string>(-1, $"No seen entry found for movie with id {id} in viewgroup {viewGroup} for date {date.ToShortDateString()}.");

            return result;
        }

        /// <summary>
        /// Get a view entry for a view group and a date.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viewGroup"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private homewebbridge_userseen GetViewEntry(int id, string viewGroup, DateTime date)
        {
            /* The context filters HomeWebUserSeen for current viewgroup in a global filter query
            /* Filters may be disabled for individual LINQ queries by using the IgnoreQueryFilters() operator.
            /* see: https://docs.microsoft.com/en-us/ef/core/querying/filters
            */
            return _context.HomeWebUserSeen.Where(m => m.vdb_videoid == id
                 // && m.asp_viewgroup == viewGroup
                 && m.viewdate.Date == date.Date).FirstOrDefault();
        }

        public async Task<Page<MovieDataResource>> GetFavoriteMoviesAsync(string userName, PagingOptions pagingOptions, CancellationToken ct)
        {
            /* The context filters HomeWebUserMovieSettings for current user in a global filter query
            /* Filters may be disabled for individual LINQ queries by using the IgnoreQueryFilters() operator.
            /* see: https://docs.microsoft.com/en-us/ef/core/querying/filters
            */
            _logger.LogTrace($"GetFavoriteMoviesAsync: Username: {userName}");
            var movieIds = _context.HomeWebUserMovieSettings
                // .Where(s => s.asp_username == userName && s.is_favorite == 1)
                .Where(s => s.is_favorite == 1)
                .Select(s => s.vdb_movieid)
                .ToList();

            return await GetMovieDataAsync(movieIds, pagingOptions, new MovieDataOptions { SortOrder = MovieDataSortOrder.ByLastSeenDateAsc, UseInlineCoverImage = true }, ct);
        }

        public async Task<Page<MovieDataResource>> GetWatchAgainMoviesAsync(string userName, PagingOptions pagingOptions, CancellationToken ct)
        {
            /* The context filters HomeWebUserMovieSettings for current user in a global filter query
             * Filters may be disabled for individual LINQ queries by using the IgnoreQueryFilters() operator.
             * see: https://docs.microsoft.com/en-us/ef/core/querying/filters
             */
            var movieIds = _context.HomeWebUserMovieSettings
                // .Where(s => s.asp_username == userName && s.watchagain == 1)
                .Where(s => s.watchagain == 1)
                .Select(s => s.vdb_movieid)
                .ToList();

            return await GetMovieDataAsync(movieIds, pagingOptions, new MovieDataOptions { UseInlineCoverImage = true, SortOrder = MovieDataSortOrder.ByLastSeenDateAsc }, ct);
        }

        public async Task<MovieDataResource> SetUnsetMovieUserFavorite(int movieId, string userName, int isFavorite, CancellationToken ct)
        {
            var entity = await InsertOrUpdateUserSetting(movieId, userName);
            entity.is_favorite = isFavorite;
            await _context.SaveChangesAsync();
            return await GetMovieDataAsync(movieId, ct);
        }

        public async Task<MovieDataResource> SetUnsetMovieUserFlagged(int movieId, string userName, int isFlagged, CancellationToken ct)
        {
            var entity = await InsertOrUpdateUserSetting(movieId, userName);
            entity.watchagain = isFlagged;
            await _context.SaveChangesAsync();
            return await GetMovieDataAsync(movieId, ct);
        }

        internal async Task<homewebbridge_usermoviesettings> InsertOrUpdateUserSetting(int movieid, string userName)
        {
            // we check, if there is already an entity, if not, we'll create one
            var entity = GetUserSetting(movieid, userName);

            if (entity == null)
            {
                var setting = new homewebbridge_usermoviesettings
                {
                    asp_username = userName,
                    vdb_movieid = movieid,
                    is_favorite = 0,
                    watchagain = 0
                };

                _context.HomeWebUserMovieSettings.Add(setting);
                entity = setting;
                await _context.SaveChangesAsync();
            }

            return entity;
        }

        /// <summary>
        /// Deletes the whole entry for user and movieid. Should just be used for tear down unit tests.
        /// </summary>
        /// <param name="movied"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        internal void DeleteCompleteUserMovieSetting(int movied, string username)
        {
            var entity = GetUserSetting(movied, username);
            if (entity != null)
            {
                _context.HomeWebUserMovieSettings.Remove(entity);
                _context.SaveChanges();
            }
        }

        private homewebbridge_usermoviesettings GetUserSetting(int movieid, string userName)
        {
            return _context.HomeWebUserMovieSettings.FirstOrDefault(m => m.vdb_movieid == movieid && m.asp_username == userName);
        }

        public async Task<Page<MovieDataResource>> GetMovieDataSurpriseAsync(int surpriseCount, MovieDataOptions movieDataOptions, CancellationToken ct)
        {
            _logger.LogTrace("Searching for surprise movies ...");
            IQueryable<videodb_videodata> query = _context.VideoData;

            query = HandleMovieDataOptions(movieDataOptions, query);

            var querysize = await query.CountAsync(ct);

            // surprise count must not be higher than size of query result
            surpriseCount = querysize < surpriseCount ? querysize : surpriseCount;

            var seed = querysize * DateTime.Now.Millisecond / 1000;
            var random = new Random(seed);

            var randomMovieIds = new List<int>();
            for (int i = 0; i < surpriseCount; i++)
            {
                var number = GenerateRandomNumber(random, querysize, randomMovieIds);
                randomMovieIds.Add(number);
            }

            var size = surpriseCount;

            // We first select the random movie Ids before loading all movie data
            var candiateIds = await query.Select(m => m.id).ToListAsync(ct);
            var selectedItemsIds = candiateIds.Where((s, i) => randomMovieIds.Contains(i)).ToList();

            var items = await _context.VideoData
                .Where(s => selectedItemsIds.Contains(s.id))
                .Include(o => o.VideoOwner)
                .Include(v => v.VideoGenres)
                .ThenInclude(g => g.Genre)
                .Include(m => m.VideoMediaType)
                .Include(s => s.SeenInformation)
                .Include(s => s.UserSettings)
                .ToListAsync(ct);

            var mappedItems = _mapper.Map<IEnumerable<MovieDataResource>>(items, opt => opt.Items[Controllers.Infrastructure.AutoMapperConstants.INLINE_COVER_IMAGE] = movieDataOptions.UseInlineCoverImage);

            return new Page<MovieDataResource>
            {
                Items = mappedItems,
                TotalSize = size
            };
        }

        private int GenerateRandomNumber(Random random, int maxValue, List<int> excludeList)
        {
            var number = random.Next(maxValue);

            if (excludeList.Contains(number))
            {
                _logger.LogTrace($"Recurse call of random number generation because of number {number}");
                number = GenerateRandomNumber(random, maxValue, excludeList);
            }
            return number;
        }

        public async Task<IEnumerable<MovieDataMediaTypeResource>> GetAllMediaTypes(CancellationToken ct)
        {
            var mediaTypePreFilter = _options.MediaTypesFilter;
            var mediaTypes = await _context.MediaTypes.Where(m => mediaTypePreFilter.Contains(m.id)).ToListAsync();
            return _mapper.Map<IEnumerable<MovieDataMediaTypeResource>>(mediaTypes);
        }

        public async Task<IEnumerable<MovieDataGenreResource>> GetAllGenres(CancellationToken ct)
        {
            var mediaTypes = await _context.Genres.ToListAsync();
            return _mapper.Map<IEnumerable<MovieDataGenreResource>>(mediaTypes);
        }

        public async Task<string> GetNextFreeDiskId(string ShelterAndCompartement)
        {
            var usedDiskIds = _context.VideoData.Where(item => item.diskid.StartsWith(ShelterAndCompartement)).Select(item => item.diskid).ToList();
            var nextDiskId = await Task.Factory.StartNew(() => _diskIdGenerator.GetNextDiskId(ShelterAndCompartement, usedDiskIds));
            return nextDiskId;
        }

        public async Task DonwloadMissingImages(CancellationToken ct, int sleepTime = 0)
        {
            foreach (var movie in _context.VideoData)
            {
                if (ct.IsCancellationRequested)
                {
                    _logger.LogTrace("Cancel running download as requested.");
                    break;
                }

                await _imageDownloader.DownloadBackgroundImageAsync(movie, sleepTime);
            }
        }

        public async Task<IEnumerable<string>> GetRacks(CancellationToken ct)
        {
            //var racks = _context.VideoData.GroupBy(d => d.diskid.Substring(0, 4));

            var query = from p in _context.VideoData
                        where !string.IsNullOrWhiteSpace(p.diskid)
                        group p by p.diskid.Substring(0, 5) into g
                        where g.Count() > 0
                        orderby g.Key
                        select new
                        {
                            g.Key
                            //,Count = g.Count()
                        };
            var racks = await query.Select(k => k.Key).ToListAsync(ct);
            return racks;
        }

        public byte[] GetCoverImageStream(int id)
        {
            var path = Path.Join(_options.LocalCoverImagePath, $"{id}.jpg");
            return imageStreamer.ReadImageFile(path);
        }

        public byte[] GetBackgroundImageStream(int id)
        {
            var path = Path.Join(_options.LocalBackgroundImagePath, $"{id}.jpg");
            return imageStreamer.ReadImageFile(path);
        }
    }
}
