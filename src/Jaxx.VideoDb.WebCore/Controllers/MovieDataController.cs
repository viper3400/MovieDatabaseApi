using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.Data.Context;
using Jaxx.VideoDb.WebCore.Infrastructure;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.VideoDb.WebCore.Services;
using Jaxx.WebApi.Shared;
using Jaxx.WebApi.Shared.Infrastructure;
using Jaxx.WebApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSwag.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Jaxx.VideoDb.WebCore.Controllers
{
    [Route("/[controller]")]
    [OpenApiTag("MovieData", Description = "ApiController to handle movie data operations.")]
    [Authorize(Policy = "VideoDbUser")]
    public class MovieDataController : Controller
    {
        private readonly IMovieDataService _movieDataService;
        private readonly PagingOptions _defaultPagingOptions;
        private readonly ILogger _logger;
        private readonly IUserContextInformationProvider userContextProvider;

        public MovieDataController(
         IMovieDataService movieDataService,
         IOptions<PagingOptions> defaultPagingOptionsAccessor,
         ILogger<MovieDataController> logger,
         IUserContextInformationProvider userContextProvider)
        {
            _movieDataService = movieDataService;
            _defaultPagingOptions = defaultPagingOptionsAccessor.Value;
            _logger = logger;
            this.userContextProvider = userContextProvider;
        }

        /**
        * @api {post} /moviedata/seen 1. Set Seen Date
        * @apiVersion 1.3.0
        * @apiName SetSeenDate
        * @apiGroup MoviesSeen
        * 
        * @apiDescription Set a seen date for a movie, the current user and it's current view group.
        * Movie id and the date have to to be passed with request content. User identiy and it's view group is
        * evaluated from Bearer Token. It's not possible to mark a movie as seen twice per day and viewgroup.
        * 
        * @apiExample Example usage:
        * http://localhost:50647/moviedata/seen
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        * 
        * @apiParam {Number} id The unique id of the movie which should be updated
        * @apiParam {Date} date the date when the movie was seen                
        * 
        * @apiParamExample {json} Request-Example:
        * {
        *    "id" : "4551",
        *    "date" : "2018-08-22",        
        * }        
        * 
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:
        *     HTTP/1.1 201 CREATED
        *  
        */
        [HttpPost("seen", Name = nameof(SetMovieSeen))]
        [OpenApiOperation("Set Seen Date", "Set a seen date for a movie, the current user and it's current view group. Movie id and the date have to to be passed with request content.User identiy and it's view group is evaluated from Bearer Token.It's not possible to mark a movie as seen twice per day and viewgroup")]
        [SwaggerResponse(200, typeof(CollectionWithPaging<Tuple<int, string>>), Description = "-")]
        [ValidateModel]
        public async Task<IActionResult> SetMovieSeen(
         [FromQuery] PagingOptions pagingOptions,
         [FromBody] MovieSeenOptions movieSeenOptions,
         CancellationToken ct)
        {

            var userName = CurrentUserName;
            var viewGroup = CurrentViewGroup;
            var options = movieSeenOptions;
            var seenDate = DateTime.Parse(options.Date);
            var result = new List<Tuple<int, string>> { await _movieDataService.MovieSeenSetAsync(options.Id, userName, viewGroup, seenDate) };

            var collection = CollectionWithPaging<Tuple<int, string>>.Create(
                Link.ToCollection(nameof(SetMovieSeen), movieSeenOptions),
                result.ToArray(),
                1,
                pagingOptions);

            return Ok(collection);

        }

        /**
        * @api {post} /moviedata/seen 2. Get all Seen Dates
        * @apiVersion 1.15.0
        * @apiName GetSeenData
        * @apiGroup MoviesSeen
        * 
        * @apiDescription Return all seen dates in desc order and the movie which was seen
        * 
        * @apiExample Example usage:
        * http://localhost:50647/moviedata/seen
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        * 
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:        
        *     HTTP/1.1 200 OK
        *  * @apiSuccessExample Success-Response:
        *     HTTP/1.1 200 OK
        *  {
        *  	"href": "http://localhost:50647/moviedata/seen",
        *  	"rel": ["collection"],
        *  	"offset": 4,
        *  	"limit": 1,
        *  	"size": 2439,
        *  	"first": {
        *  		"href": "http://localhost:50647/moviedata/seen",
        *  		"rel": ["collection"]
        *  	},
        *  	"previous": {
        *  		"href": "http://localhost:50647/moviedata?limit=1&offset=3",
        *  		"rel": ["collection"]
        *  	},
        *  	"next": {
        *  		"href": "http://localhost:50647/moviedata/seen?limit=1&offset=5",
        *  		"rel": ["collection"]
        *  	},
        *  	"last": {
        *  		"href": "http://localhost:50647/moviedata/seen?limit=1&offset=2438",
        *  		"rel": ["collection"]
        *  	},
        *  	"value": [{
        *  		"href": "http://localhost:50647/moviedata/2245",
        *  		"seenDate": "2019-06-02T00:00:00Z",
        *  		"movie": { MovieDataRessource }
        *  	}]
        *  } 
        */
        [HttpGet("seen", Name = nameof(GetMovieSeen))]
        [OpenApiOperation("Get all Seen Dates", "Return all seen dates in desc order and the movie which was seen.")]
        [SwaggerResponse(200, typeof(CollectionWithPagingAndDateFilter<MovieDataSeenResource>), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public async Task<IActionResult> GetMovieSeen(
        [FromQuery] PagingOptions pagingOptions,
        [FromQuery] DateRangeFilterOptions dateRangeFilterOptions,
        CancellationToken ct)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var movies = await _movieDataService.GetSeenMovies(pagingOptions, dateRangeFilterOptions, ct);

            var collection = CollectionWithPagingAndDateFilter<MovieDataSeenResource>.Create(
                Link.ToCollection(nameof(GetMovieSeen)),
                movies.Items.ToArray(),
                movies.TotalSize,
                pagingOptions,
                dateRangeFilterOptions);

            return Ok(collection);
        }

        /**
       * @api {delete} /moviedata/seen 3. Remove Seen Date
       * @apiVersion 1.3.0
       * @apiName RemoveSeenDate
       * @apiGroup MoviesSeen
       * 
       *  @apiExample Example usage:
       * http://localhost:50647/moviedata/seen
       * 
       * @apiHeader {String} Content-Type Request type, must be "application/json".
       * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
       * @apiHeaderExample {json} Request-Example:
       * {
       *   "Content-Type": "application/json"
       *   "Authorization": "Bearer ewrjfjfoweffefo98098"
       * }
       * 
       * @apiParam {Number} id The unique id of the movie which should be updated
       * @apiParam {Date} date the date when the movie was seen        
       * 
       * @apiParamExample {json} Request-Example:
       * {
       *    "id" : "4551",
       *    "date" : "2018-08-22",
       * }        
       * 
       * @apiError 401 Unauthorized
       * @apiSuccessExample Success-Response:
       *     HTTP/1.1 201 CREATED
       *  
       */
        [HttpDelete("seen", Name = nameof(UnSetMovieSeen))]
        [ValidateModel]
        public async Task<IActionResult> UnSetMovieSeen(
        [FromQuery] PagingOptions pagingOptions,
        [FromBody] MovieSeenOptions movieSeenOptions,
        CancellationToken ct)
        {

            var userName = CurrentUserName;
            var viewGroup = CurrentViewGroup;
            var options = movieSeenOptions;
            var seenDate = DateTime.Parse(options.Date);
            var result = new List<Tuple<int, string>> { await _movieDataService.MovieSeenDeleteAsync(options.Id, viewGroup, seenDate) };

            var collection = CollectionWithPaging<Tuple<int, string>>.Create(
                Link.ToCollection(nameof(SetMovieSeen), movieSeenOptions),
                result.ToArray(),
                1,
                pagingOptions);

            return Ok(collection);

        }

        /**
       * @api {get} /moviedata 1a. Get all movies
       * @apiVersion 1.12.0
       * @apiName GetMovies
       * @apiGroup MoviesData
       * @apiGroup MoviesData
       * 
       * @apiParam {Number} [limit] Paging option. Maximum number of returned stakeholders per page.
       * @apiParam {Number} [offset] Paging option. The offset from where the API will start to return stakeholders.
       *                              Must be greater than 0.
       * @apiParam {string} [diskid] API will search for the given disk id.
       * @apiParam {string} [title] API will search for the given title (in titles and subtitles).
       * @apiParam {string} [search] API will check if a string like a Disk Id is given (RxxFx) and will search by diskid, otherwhise a
       *                               search by title is performed
       * @apiParam {number} [mediatypes] API will search movies with this mediatypes (comma separated list)
       * @apiParam {string} [genres] Comma separated list with (existing) genre names.
       * @apiParam {Boolean} [istv] Filters movies marked as istv if set to true and removes them from result when set to false.
       * @apiParam {Boolean} [isdeleted] Filters movies marked as deleted if set to true and removes them from result when set to false
       * @apiParam {Boolean} [notseen] Filters movies marked as not seen or seen in current viewgroup (true/false)
       * 
       * @apiExample Example usage:
       * http://localhost:50647/moviedata
       * http://localhost:50647/moviedata?limit=5&offset=5 
       * http://localhost:50647/moviedata?search=Batman&limit=5&offset=5
       * 
       * @apiHeader {String} Content-Type Request type, must be "application/json".
       * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
       * @apiHeaderExample {json} Request-Example:
       * {
       *   "Content-Type": "application/json"
       *   "Authorization": "Bearer ewrjfjfoweffefo98098"
       * }
       * @apiError 401 Unauthorized
       * @apiSuccessExample Success-Response:
       *     HTTP/1.1 200 OK
       *  {
       *  	"href": "http://localhost:50647/moviedata",
       *  	"rel": ["collection"],
       *  	"offset": 4,
       *  	"limit": 1,
       *  	"size": 2439,
       *  	"first": {
       *  		"href": "http://localhost:50647/moviedata",
       *  		"rel": ["collection"]
       *  	},
       *  	"previous": {
       *  		"href": "http://localhost:50647/moviedata?limit=1&offset=3",
       *  		"rel": ["collection"]
       *  	},
       *  	"next": {
       *  		"href": "http://localhost:50647/moviedata?limit=1&offset=5",
       *  		"rel": ["collection"]
       *  	},
       *  	"last": {
       *  		"href": "http://localhost:50647/moviedata?limit=1&offset=2438",
       *  		"rel": ["collection"]
       *  	},
       *  	"value": [{
       *  		"href": "http://localhost:50647/moviedata/2245",
       *  		"id": 2245,
       *  		"md5": null,
       *  		"title": "SpongeBob Schwammkopf - Der Film",
       *  		"subtitle": null,
       *  		"language": null,
       *  		"diskid": "R01F1D05",
       *  		"comment": null,
       *  		"disklabel": null,
       *  		"imdbID": "ofdb:62189-366760",
       *  		"year": 2004,
       *  		"imgurl": "./coverpics/2245.jpg",
       *  		"director": null,
       *  		"actors": "David Hasselhoff::::ofdb:0\r\nKristopher Logan::::ofdb:0\r\nD.P. FitzGerald::::ofdb:0\r\nCole S. McKay::::ofdb:0\r\nDylan Haggerty::::ofdb:0\r\nBart McCarthy::::ofdb:0\r\nHenry Kingi::::ofdb:0\r\nRandolph Jones::::ofdb:0\r\nPaul Zies::::ofdb:0\r\nGerard Griesbaum::::ofdb:0\r\nAaron Hendry::::ofdb:0\r\nMaxie J. Santillan Jr.::::ofdb:0",
       *  		"runtime": 87,
       *  		"country": "USA",
       *  		"plot": "Hektik in Bikini Bottom, dem Wohnort des gelben Schwamms Spongebob Schwammkopf. Irgendjemand hat die Krone von König Neptun gestohlen und der Verdacht fällt sofort auf Spongbobs Chef Mr. Krabs. Nur Spongebob und sein bester Freund, der Seestern Patrick, glauben an Mr. Krabs und machen sich auf, um dessen Unschuld zu beweisen. Dafür müssen sie Bikini Bottom verlassen und sich nach Shell City durchschlagen. Doch auf dem Weg dorthin kommen sie in allerlei schwierige Situation, die überstanden werden wollen, um Mr. Krabs zu retten.",
       *  		"rating": null,
       *  		"filename": null,
       *  		"filesize": null,
       *  		"filedate": null,
       *  		"audio_codec": null,
       *  		"video_codec": null,
       *  		"video_width": null,
       *  		"video_height": null,
       *  		"istv": false,
       *  		"lastupdate": "2017-12-15T17:13:13Z",
       *  		"mediatype": 16,
       *  		"custom1": null,
       *  		"custom2": "4010884250022",
       *  		"custom3": "http://img.ofdb.de/film/62/62189.jpg",
       *  		"custom4": null,
       *  		"created": "2015-05-10T13:10:06Z",
       *  		"owner_id": 999,
       *  		"videoOwner": "DELETED",
       *  		"genres": [{
       *  			"id": 2,
       *  			"name": "Adventure"
       *  		},
       *  		{
       *  			"id": 4,
       *  			"name": "Comedy"
       *  		},
       *  		{
       *  			"id": 8,
       *  			"name": "Family"
       *  		}],
       *  		"lastSeenInformation": {
       *  			"lastSeenDate": "2017-12-13T21:32:09Z",
       *  			"seenCount": 1,
       *  			"daysSinceLastView": 168,
       *  			"lastSeenSentence": "Du hast diesen Film bereits 1 Mal gesehen, zuletzt am Mittwoch, 13. Dezember 2017. Das war vor 168 Tagen.",
       *  			"readableTimeSinceLastViewHtml": "1&frac12;Y",
       *  			"allSeenDates": ["2017-12-13T21:32:09Z", "2014-03-23T21:48:29Z", "2013-09-19T00:00:00Z"]
       *  		},
       *	    "isFavorite": true,
       *        "isFlagged": false
       *  	}]
       *  }
       */
        [HttpGet(Name = nameof(GetMovieDataAsync))]
        [ValidateModel]
        public async Task<IActionResult> GetMovieDataAsync(
         [FromQuery] PagingOptions pagingOptions,
         [FromQuery] MovieDataOptions movieDataOptions,
         CancellationToken ct)
        {
            try
            {
                pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
                pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

                var movies = await _movieDataService.GetMovieDataAsync(null, pagingOptions, movieDataOptions, ct);

                var collection = CollectionWithPaging<MovieDataResource>.Create(
                    Link.ToCollection(nameof(GetMovieDataAsync), movieDataOptions),
                    movies.Items.ToArray(),
                    movies.TotalSize,
                    pagingOptions);

                if (ct.IsCancellationRequested) throw new OperationCanceledException();

                return Ok(collection);
            }
            catch (OperationCanceledException canceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogTrace(canceledException.Message);
                return NoContent();
            }
        }

        /**
        * @api {post} /moviedata 1b. Get all movies
        * @apiVersion 1.4.0
        * @apiName GetMoviesPostFilter
        * @apiGroup MoviesData
        * @apiGroup MoviesData
        * 
        * @apiParam {Number} [limit] Paging option. Maximum number of returned stakeholders per page.
        * @apiParam {Number} [offset] Paging option. The offset (stakeholders) from where the API will start to return stakeholders.
        *                              Must be greater than 0.
        * @apiParam {string} [diskid] API will search for the given disk id.
        * @apiParam {string} [title] API will search for the given title (in titles and subtitles).
        * @apiParam {string} [search] API will check if a string like a Disk Id is given (RxxFx) and will search by diskid, otherwhise a
        *                               search by title is performed
        * 
        * @apiExample Example usage:
        * http://localhost:50647/moviedata
        * http://localhost:50647/moviedata?limit=5&offset=5         
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        * 
        * @apiParamExample {json} Request-Example:
        * {
        *    "diskid" : "R20",
        *    "title" : "my",        
        * } 
        * 
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:
        *     HTTP/1.1 200 OK
        *  {
        *  	 ... see the GET example
        *  }
        */
        [HttpPost(Name = nameof(GetMovieDataWithFilterAsync))]
        [ValidateModel]
        public async Task<IActionResult> GetMovieDataWithFilterAsync(
         [FromQuery] PagingOptions pagingOptions,
         [FromBody] MovieDataOptions movieDataOptions,
         CancellationToken ct)
        {
            try
            {
                pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
                pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;
                var movies = await _movieDataService.GetMovieDataAsync(null, pagingOptions, movieDataOptions, ct);

                var collection = CollectionWithPaging<MovieDataResource>.Create(
                    Link.ToCollection(nameof(GetMovieDataWithFilterAsync), movieDataOptions),
                    movies.Items.ToArray(),
                    movies.TotalSize,
                    pagingOptions);

                return Ok(collection);
            }
            catch (OperationCanceledException canceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogTrace(canceledException.Message);
                return NoContent();
            }
        }

        /**
        * @api {get} /moviedata 1c. Get surprise movie
        * @apiVersion 1.12.0
        * @apiName GetSurpriseMovies
        * @apiGroup MoviesData
        * @apiGroup MoviesData
        * 
        * @apiParam {Number} [limit] Number of surprise videos.
        * @apiParam {string} [diskid] API will search for the given disk id.
        * @apiParam {string} [title] API will search for the given title (in titles and subtitles).
        * @apiParam {string} [search] API will check if a string like a Disk Id is given (RxxFx) and will search by diskid, otherwhise a
        *                               search by title is performed
        * @apiParam {number} [mediatypes] API will search movies with this mediatypes (comma separated list)
        * @apiParam {string} [genres] Comma separated list with (existing) genre names.
        * @apiParam {Boolean} [istv] Filters movies marked as istv if set to true and removes them from result when set to false.
        * @apiParam {Boolean} [isdeleted] Filters movies marked as deleted if set to true and removes them from result when set to false
        * @apiParam {string} [notseen] Filters movies marked as not seen in current viewgroup since the given numver of days   
        *  
        * @apiExample Example usage:
        * http://localhost:50647/moviedata
        * http://localhost:50647/moviedata?limit=5&offset=5 
        * http://localhost:50647/moviedata?search=Batman&limit=5&offset=5
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:
        *     HTTP/1.1 200 OK
        *  {
        *     [Paged movie data ressoure]
        *  }
        *  */
        [HttpGet("surprise", Name = nameof(GetMovieDataSurpriseAsync))]
        [ValidateModel]
        public async Task<IActionResult> GetMovieDataSurpriseAsync(
        [FromQuery] PagingOptions pagingOptions,
        [FromQuery] MovieDataOptions movieDataOptions,
        CancellationToken ct)
        {
            try
            {
                pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;
                pagingOptions.Offset = 0;

                var movies = await _movieDataService.GetMovieDataSurpriseAsync((int)pagingOptions.Limit, movieDataOptions, ct);

                var collection = CollectionWithPaging<MovieDataResource>.Create(
                    Link.ToCollection(nameof(GetMovieDataSurpriseAsync), movieDataOptions),
                    movies.Items.ToArray(),
                    movies.TotalSize,
                    pagingOptions);

                if (ct.IsCancellationRequested) throw new OperationCanceledException();

                return Ok(collection);
            }
            catch (OperationCanceledException canceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogTrace(canceledException.Message);
                return NoContent();
            }
        }

        [HttpGet("experiment", Name = nameof(GetMovieDataExperimentAsync))]
        [ValidateModel]
        public async Task<IActionResult> GetMovieDataExperimentAsync(
         [FromQuery] PagingOptions pagingOptions,
         [FromQuery] MovieDataOptions movieDataOptions,
         CancellationToken ct)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var movies = await _movieDataService.GetMovieDataEnhancedAsync(null, pagingOptions, movieDataOptions, ct);

            var collection = CollectionWithPaging<MovieDataEnhancedResource>.Create(
                Link.ToCollection(nameof(GetMovieDataExperimentAsync)),
                movies.Items.ToArray(),
                movies.TotalSize,
                pagingOptions);

            return Ok(collection);
        }

        [HttpGet("bgimg", Name = nameof(GetMovieDataBgImg))]
        [ValidateModel]
        public async Task<IActionResult> GetMovieDataBgImg(
         [FromQuery] PagingOptions pagingOptions,
         [FromQuery] MovieDataOptions movieDataOptions,
         CancellationToken ct)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var movie = System.Net.WebUtility.UrlEncode(movieDataOptions.Title);

            var httpClient = new System.Net.Http.HttpClient();
            httpClient.BaseAddress = new System.Uri("http://api.themoviedb.org");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var result = await httpClient.GetAsync($"/3/search/movie/?api_key=a0f65d2a901a81eadf0bd7a57be924ac&language=de-DE&query={movie}");
            var content = await result.Content.ReadAsStringAsync();

            var collection = content;

            return Ok(collection);
        }

        [HttpGet("loadBgImg", Name = nameof(LoadBgImg))]
        [ValidateModel]
        public async Task<IActionResult> LoadBgImg(
            [FromQuery] PagingOptions pagingOptions,
            [FromQuery] MovieDataOptions movieDataOptions,
            CancellationToken ct)
        {
            await _movieDataService.DonwloadMissingImages(ct, 200);
            return Ok();
        }
        /**
        * @api {get} /moviedata/:movieId 2. Get movie by id
        * @apiVersion 1.7.0
        * @apiName Get Movie By Id
        * @apiGroup MoviesData
        * 
        * @apiParam {Number} movieId Unique id of movie.         
        *
        * @apiExample Example usage:
        * http://localhost:50647/moviedata/2011
        *
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:
        *     HTTP/1.1 200 OK
        *  {
        *    "href": "https://danielgraefe.de/api/videodb/beta/moviedata/2011",
        *    "id": 2011,
        *    "md5": "",
        *    "title": "Tage am Strand",
        *    "subtitle": "",
        *    "language": "Deutsch",
        *    "diskid": "R20F8D07",
        *    "comment": "",
        *    "disklabel": "",
        *    "imdbID": "ofdb:236996-50933",
        *    "year": 2013,
        *    "imgurl": "./coverpics/2011.jpg",
        *    "director": "Anne Fontaine",
        *    "actors": "Naomi Watts::Lil::ofdb:1271\nRobin Wright::::\nBen Mendelsohn::Harold::ofdb:51194\nXavier Samuel::Ian::ofdb:41117\nJames Frecheville::::\nSophie Lowe::Hannah::ofdb:58507\nGary Sweet::::\nJessica Tovey::::\nAlyson Standen::::\nDane Eade::::\nCharlee Thomas::::\nScott Pirlo::::",
        *    "runtime": null,
        *    "country": "Australien",
        *    "plot": "",
        *    "rating": "",
        *    "filename": "",
        *    "filesize": null,
        *    "filedate": null,
        *    "audio_codec": "",
        *    "video_codec": "",
        *    "video_width": null,
        *    "video_height": null,
        *    "istv": false,
        *    "lastupdate": "2014-05-09T18:47:54Z",
        *    "mediatype": 16,
        *    "custom1": "",
        *    "custom2": "4010324039668",
        *    "custom3": "http://img.ofdb.de/film/236/236996.jpg",
        *    "custom4": "",
        *    "created": "2014-05-09T18:35:09Z",
        *    "owner_id": 3,
        *    "videoOwner": "Daniel",
        *    "genres": [{
        *      "id": 7,
        *      "name": "Drama"
        *    }],
        *    "lastSeenInformation": null,
        *    "isFavorite": true,
        *    "isFlagged": false
        *  }
*/
        [HttpGet("{movieId}", Name = nameof(GetMovieDataByIdAsync))]
        [ValidateModel]
        public async Task<IActionResult> GetMovieDataByIdAsync(GetMovieByIdParameters parameters, CancellationToken ct)
        {
            if (parameters.MovieId == 0) return NotFound();

            var movie = await _movieDataService.GetMovieDataAsync(parameters.MovieId, ct);
            if (movie == null) return NotFound();

            return Ok(movie);
        }

        /**
        * @api {get} /moviedata/favorites 1. Get favorite movies
        * @apiVersion 1.3.0
        * @apiName GetFavoriteMovie
        * @apiGroup MoviesFavorite
        * 
        * @apiParam {Number} [limit] Paging option. Maximum number of returned stakeholders per page.
        * @apiParam {Number} [offset] Paging option. The offset (stakeholders) from where the API will start to return stakeholders.
        *                              Must be greater than 0.
        * 
        * @apiExample Example usage:
        * http://localhost:50647/moviedata/favorites
        * http://localhost:50647/moviedata/favorites/?limit=5&offset=5 
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:
        *     HTTP/1.1 200 OK
        *  {
        *  	"href": "http://localhost:50647/moviedata",
        *  	"rel": ["collection"],
        *  	"offset": 4,
        *  	"limit": 1,
        *  	"size": 2439,
        *  	"first": {
        *  		"href": "http://localhost:50647/moviedata",
        *  		"rel": ["collection"]
        *  	},
        *  	"previous": {
        *  		"href": "http://localhost:50647/moviedata?limit=1&offset=3",
        *  		"rel": ["collection"]
        *  	},
        *  	"next": {
        *  		"href": "http://localhost:50647/moviedata?limit=1&offset=5",
        *  		"rel": ["collection"]
        *  	},
        *  	"last": {
        *  		"href": "http://localhost:50647/moviedata?limit=1&offset=2438",
        *  		"rel": ["collection"]
        *  	},
        *  	"value": [{
        *  		"href": "http://localhost:50647/moviedata/2245",
        *  		"id": 2245,
        *  		"md5": null,
        *  		"title": "SpongeBob Schwammkopf - Der Film",
        *  		"subtitle": null,
        *  		"language": null,
        *  		"diskid": "R01F1D05",
        *  		"comment": null,
        *  		"disklabel": null,
        *  		"imdbID": "ofdb:62189-366760",
        *  		"year": 2004,
        *  		"imgurl": "./coverpics/2245.jpg",
        *  		"director": null,
        *  		"actors": "David Hasselhoff::::ofdb:0\r\nKristopher Logan::::ofdb:0\r\nD.P. FitzGerald::::ofdb:0\r\nCole S. McKay::::ofdb:0\r\nDylan Haggerty::::ofdb:0\r\nBart McCarthy::::ofdb:0\r\nHenry Kingi::::ofdb:0\r\nRandolph Jones::::ofdb:0\r\nPaul Zies::::ofdb:0\r\nGerard Griesbaum::::ofdb:0\r\nAaron Hendry::::ofdb:0\r\nMaxie J. Santillan Jr.::::ofdb:0",
        *  		"runtime": 87,
        *  		"country": "USA",
        *  		"plot": "Hektik in Bikini Bottom, dem Wohnort des gelben Schwamms Spongebob Schwammkopf. Irgendjemand hat die Krone von König Neptun gestohlen und der Verdacht fällt sofort auf Spongbobs Chef Mr. Krabs. Nur Spongebob und sein bester Freund, der Seestern Patrick, glauben an Mr. Krabs und machen sich auf, um dessen Unschuld zu beweisen. Dafür müssen sie Bikini Bottom verlassen und sich nach Shell City durchschlagen. Doch auf dem Weg dorthin kommen sie in allerlei schwierige Situation, die überstanden werden wollen, um Mr. Krabs zu retten.",
        *  		"rating": null,
        *  		"filename": null,
        *  		"filesize": null,
        *  		"filedate": null,
        *  		"audio_codec": null,
        *  		"video_codec": null,
        *  		"video_width": null,
        *  		"video_height": null,
        *  		"istv": false,
        *  		"lastupdate": "2017-12-15T17:13:13Z",
        *  		"mediatype": 16,
        *  		"custom1": null,
        *  		"custom2": "4010884250022",
        *  		"custom3": "http://img.ofdb.de/film/62/62189.jpg",
        *  		"custom4": null,
        *  		"created": "2015-05-10T13:10:06Z",
        *  		"owner_id": 999,
        *  		"videoOwner": "DELETED",
        *  		"genres": [{
        *  			"id": 2,
        *  			"name": "Adventure"
        *  		},
        *  		{
        *  			"id": 4,
        *  			"name": "Comedy"
        *  		},
        *  		{
        *  			"id": 8,
        *  			"name": "Family"
        *  		}],
        *  		"lastSeenInformation": {
        *  			"lastSeenDate": "2017-12-13T21:32:09Z",
        *  			"seenCount": 1,
        *  			"daysSinceLastView": 168,
        *  			"lastSeenSentence": "Du hast diesen Film bereits 3 Mal gesehen, zuletzt am Mittwoch, 13. Dezember 2017. Das war vor 168 Tagen.",
        *  			"readableTimeSinceLastViewHtml": "5Y",
        *  			"allSeenDates": ["2017-12-13T21:32:09Z", "2014-03-23T21:48:29Z", "2013-09-19T00:00:00Z"]
        *  		}
        *  	}]
        *  }
        */
        [HttpGet("favorites", Name = nameof(GetFavoriteMoviesAsync))]
        [OpenApiOperation("Get favorite movies", "Get favourite movies for current user.")]
        [NSwag.Annotations.OpenApiExtensionData("x", "y")]
        [SwaggerResponse(200, typeof(CollectionWithPaging<MovieDataResource>), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public async Task<IActionResult> GetFavoriteMoviesAsync(
        [FromQuery] PagingOptions pagingOptions,
            CancellationToken ct)
        {
            _logger.LogDebug("{0}|Get favorite movies for user {1}", nameof(GetFavoriteMoviesAsync), userContextProvider.UserName);
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var movies = await _movieDataService.GetFavoriteMoviesAsync(CurrentUserName, pagingOptions, ct);

            var collection = CollectionWithPaging<MovieDataResource>.Create(
                Link.ToCollection(nameof(GetFavoriteMoviesAsync)),
                movies.Items.ToArray(),
                movies.TotalSize,
                pagingOptions);

            return Ok(collection);
        }

        /**
        * @api {post} /moviedata/:id 3. Update/create movie
        * @apiVersion 1.6.0
        * @apiName UpdateOrCreateMovie
        * @apiGroup MoviesData
        * 
        * @apiDescription Updates or creates a movie with a resource. When id in route is 0 a new movie is 
        *        created from ressource.
        *        In case a valid id >0 is passed the movie with this id will be updated.
        *        The resource to update need to be complete, because the original entity will
        *        be overwritten completely. The id, diskid and created field won't be touched by this operation.
        *        LastUpdated will be set to now.
        * 
        * @apiParam {int} id Id of the movie to update. If emtpy, a new movie will be created.
        * @apiParam {json} MovieDataResource Takes a MovieDataResource as Json Body (see Result of GetMovieById)
        * 
        * @apiExample Example usage:
        * http://localhost:50647/moviedata/5
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        *  @apiParamExample {json} Request-Example:
        *     {
        *       "id" : 5,
        *       "title": "Tage am Strand",
        *       "subtitle": "",
        *       "language": "Deutsch",
        *       "diskid": "R20F8D07",
        *       [...]
        *       // EXAMPLE INCOMPLETE !
        *     }
        *     
        * @apiError 401 Unauthorized
        * @apiError 400 BadRequest
        * @apiSuccessExample Success-Response:        
        *     HTTP/1.1 200 OK
        *  
        */
        [HttpPost("{movieId}", Name = nameof(UpdateOrCreateMovieDataByIdAsync))]
        [ValidateModel]
        public async Task<IActionResult> UpdateOrCreateMovieDataByIdAsync(
            GetMovieByIdParameters parameters,
            [FromBody] MovieDataResource movieDataResource,
            CancellationToken ct)
        {
            if (parameters.MovieId == 0 && movieDataResource.id > 0) return NotFound();
            else if (parameters.MovieId == 0)
            {
                var createdMovie = await _movieDataService.CreateMovieDataAsync(movieDataResource, ct);
                return Ok(createdMovie);
            }
            else
            {
                var movie = await _movieDataService.GetMovieDataAsync(parameters.MovieId, ct);
                if (movie == null) return NotFound();

                var updatedMovie = await _movieDataService.UpdateMovieDataAsync(parameters.MovieId, movieDataResource, ct);
                return Ok(updatedMovie);
            }
        }

        /**
        * @api {get} /moviedata/watchagain 1. Get watch again movies
        * @apiVersion 1.3.0
        * @apiName GetWatchAgain Movies
        * @apiGroup MoviesWatchAgain
        * 
        * @apiParam {Number} [limit] Paging option. Maximum number of returned stakeholders per page.
        * @apiParam {Number} [offset] Paging option. The offset (stakeholders) from where the API will start to return stakeholders.
        *                              Must be greater than 0.
        * 
        * @apiExample Example usage:
        * http://localhost:50647/moviedata/watchagain
        * http://localhost:50647/moviedata/watchagain/?limit=5&offset=5 
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:
        *     HTTP/1.1 200 OK
        *  {
        *  	"href": "http://localhost:50647/moviedata",
        *  	"rel": ["collection"],
        *  	"offset": 4,
        *  	"limit": 1,
        *  	"size": 2439,
        *  	"first": {
        *  		"href": "http://localhost:50647/moviedata",
        *  		"rel": ["collection"]
        *  	},
        *  	"previous": {
        *  		"href": "http://localhost:50647/moviedata?limit=1&offset=3",
        *  		"rel": ["collection"]
        *  	},
        *  	"next": {
        *  		"href": "http://localhost:50647/moviedata?limit=1&offset=5",
        *  		"rel": ["collection"]
        *  	},
        *  	"last": {
        *  		"href": "http://localhost:50647/moviedata?limit=1&offset=2438",
        *  		"rel": ["collection"]
        *  	},
        *  	"value": [{
        *  		"href": "http://localhost:50647/moviedata/2245",
        *  		"id": 2245,
        *  		"md5": null,
        *  		"title": "SpongeBob Schwammkopf - Der Film",
        *  		"subtitle": null,
        *  		"language": null,
        *  		"diskid": "R01F1D05",
        *  		"comment": null,
        *  		"disklabel": null,
        *  		"imdbID": "ofdb:62189-366760",
        *  		"year": 2004,
        *  		"imgurl": "./coverpics/2245.jpg",
        *  		"director": null,
        *  		"actors": "David Hasselhoff::::ofdb:0\r\nKristopher Logan::::ofdb:0\r\nD.P. FitzGerald::::ofdb:0\r\nCole S. McKay::::ofdb:0\r\nDylan Haggerty::::ofdb:0\r\nBart McCarthy::::ofdb:0\r\nHenry Kingi::::ofdb:0\r\nRandolph Jones::::ofdb:0\r\nPaul Zies::::ofdb:0\r\nGerard Griesbaum::::ofdb:0\r\nAaron Hendry::::ofdb:0\r\nMaxie J. Santillan Jr.::::ofdb:0",
        *  		"runtime": 87,
        *  		"country": "USA",
        *  		"plot": "Hektik in Bikini Bottom, dem Wohnort des gelben Schwamms Spongebob Schwammkopf. Irgendjemand hat die Krone von König Neptun gestohlen und der Verdacht fällt sofort auf Spongbobs Chef Mr. Krabs. Nur Spongebob und sein bester Freund, der Seestern Patrick, glauben an Mr. Krabs und machen sich auf, um dessen Unschuld zu beweisen. Dafür müssen sie Bikini Bottom verlassen und sich nach Shell City durchschlagen. Doch auf dem Weg dorthin kommen sie in allerlei schwierige Situation, die überstanden werden wollen, um Mr. Krabs zu retten.",
        *  		"rating": null,
        *  		"filename": null,
        *  		"filesize": null,
        *  		"filedate": null,
        *  		"audio_codec": null,
        *  		"video_codec": null,
        *  		"video_width": null,
        *  		"video_height": null,
        *  		"istv": false,
        *  		"lastupdate": "2017-12-15T17:13:13Z",
        *  		"mediatype": 16,
        *  		"custom1": null,
        *  		"custom2": "4010884250022",
        *  		"custom3": "http://img.ofdb.de/film/62/62189.jpg",
        *  		"custom4": null,
        *  		"created": "2015-05-10T13:10:06Z",
        *  		"owner_id": 999,
        *  		"videoOwner": "DELETED",
        *  		"genres": [{
        *  			"id": 2,
        *  			"name": "Adventure"
        *  		},
        *  		{
        *  			"id": 4,
        *  			"name": "Comedy"
        *  		},
        *  		{
        *  			"id": 8,
        *  			"name": "Family"
        *  		}],
        *  		"lastSeenInformation": {
        *  			"lastSeenDate": "2017-12-13T21:32:09Z",
        *  			"seenCount": 1,
        *  			"daysSinceLastView": 168,
        *  			"lastSeenSentence": "Du hast diesen Film bereits 1 Mal gesehen, zuletzt am Mittwoch, 13. Dezember 2017. Das war vor 168 Tagen."
        *  		}
        *  	}]
        *  }
        */
        [HttpGet("watchagain", Name = nameof(GetWatchAgainMoviesAsync))]
        [ValidateModel]
        public async Task<IActionResult> GetWatchAgainMoviesAsync(
        [FromQuery] PagingOptions pagingOptions,
            CancellationToken ct)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var movies = await _movieDataService.GetWatchAgainMoviesAsync(CurrentUserName, pagingOptions, ct);

            var collection = CollectionWithPaging<MovieDataResource>.Create(
                Link.ToCollection(nameof(GetWatchAgainMoviesAsync)),
                movies.Items.ToArray(),
                movies.TotalSize,
                pagingOptions);

            return Ok(collection);
        }

        /**
        * @api {post} /moviedata/watchagain/:movieid 2. Set watch again movies
        * @apiVersion 1.8.0
        * @apiName SetWatchAgain Movie
        * @apiGroup MoviesWatchAgain
        * 
        * @apiParam {Number} [movied] Id of the movie
        * @apiParam {Boolean} [isFlagged] true, if movie is flagged, false if not.
        * 
        * @apiExample Example usage:
        * http://localhost:50647/moviedata/watchagain
        * http://localhost:50647/moviedata/watchagain/?limit=5&offset=5 
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        *  @apiParamExample {json} Request-Example:
        *     {
        *       "isFlagged" : true
        *     }
        *     
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:
        *     HTTP/1.1 200 OK
        *  {
        *       // MovieDataRessource
        *  }
        */
        [HttpPost("watchagain/{movieId}", Name = nameof(SetWatchAgainMoviesAsync))]
        [ValidateModel]
        public async Task<IActionResult> SetWatchAgainMoviesAsync(
        GetMovieByIdParameters parameters,
        [FromBody] MovieUserSetting movieUserSettings,
            CancellationToken ct)
        {
            var isFlagged = movieUserSettings.IsFlagged ? 1 : 0;
            var movie = await _movieDataService.SetUnsetMovieUserFlagged(parameters.MovieId, CurrentUserName, isFlagged, ct);
            return Ok(movie);
        }


        /**
        * @api {post} /moviedata/favorites/:movieid 2. Set favorite movie
        * @apiVersion 1.8.0
        * @apiName SetFavorite Movie
        * @apiGroup MoviesFavorite
        * 
        * @apiParam {Number} [movied] Id of the movie
        * @apiParam {Boolean} [isFavorite] true, if movie is favorite, false if not.
        * 
        * @apiExample Example usage:
        * http://localhost:50647/moviedata/watchagain
        * http://localhost:50647/moviedata/watchagain/?limit=5&offset=5 
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        *  @apiParamExample {json} Request-Example:
        *     {
        *       "isFavorite" : true
        *     }
        *     
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:
        *     HTTP/1.1 200 OK
        *  {
        *       // MovieDataRessource
        *  }
        */
        [HttpPost("favorites/{movieId}", Name = nameof(SetFavoriteMoviesAsync))]
        [ValidateModel]
        public async Task<IActionResult> SetFavoriteMoviesAsync(
        GetMovieByIdParameters parameters,
        [FromBody] MovieUserSetting movieUserSettings,
            CancellationToken ct)
        {
            var isFavorite = movieUserSettings.IsFavorite ? 1 : 0;
            var movie = await _movieDataService.SetUnsetMovieUserFavorite(parameters.MovieId, CurrentUserName, isFavorite, ct);
            return Ok(movie);
        }


        /**
        * @api {get} /moviedata/mediatypes 1. Get all media types
        * @apiVersion 1.11.0
        * @apiName Get all media types
        * @apiGroup MediaTypes
        * 
        * @apiExample Example usage:
        * http://localhost:50647/mediatypes
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        *  @apiParamExample {json} Request-Example:
        *     {
        *       "isFavorite" : true
        *     }
        *     
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:
        *     HTTP/1.1 200 OK
        *  {
        *      {
        *      "href": "https://danielgraefe.de/api/videodb/beta/moviedata/mediatypes",
        *      "rel": ["collection"],
        *      "offset": 0,
        *      "limit": 100,
        *      "size": 3,
        *     "first": {
        *        "href": "https://danielgraefe.de/api/videodb/beta/moviedata/mediatypes",
        *        "rel": ["collection"]
        *      },
        *      "value": [{
        *        "href": null,
        *        "id": 1,
        *        "name": "DVD"
        *      }, {
        *        "href": null,
        *        "id": 2,
        *        "name": "SVCD"
        *     }, {
        *        "href": null,
        *        "id": 3,
        *        "name": "VCD"
        *      }]
        *    }
        *  }
        */
        [HttpGet("mediatypes", Name = nameof(GetAllMediaTypes))]
        public async Task<IActionResult> GetAllMediaTypes(CancellationToken ct)
        {
            var types = await _movieDataService.GetAllMediaTypes(ct);

            var collection = CollectionWithPaging<MovieDataMediaTypeResource>.Create(
             Link.ToCollection(nameof(GetAllMediaTypes)),
             types.ToArray(),
             types.Count(),
             new PagingOptions { Limit = 100, Offset = 0 });

            return Ok(collection);
        }

        /**
        * @api {get} /moviedata/genres 1. Get all genres
        * @apiVersion 1.11.0
        * @apiName Get all genres
        * @apiGroup Genres
        * 
        * @apiExample Example usage:
        * http://localhost:50647/genres
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        *  @apiParamExample {json} Request-Example:
        *     {
        *       "isFavorite" : true
        *     }
        *     
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:
        *     HTTP/1.1 200 OK
        *  {
        *      {
        *      "href": "https://danielgraefe.de/api/videodb/beta/moviedata/genres",
        *      "rel": ["collection"],
        *      "offset": 0,
        *      "limit": 100,
        *      "size": 3,
        *     "first": {
        *        "href": "https://danielgraefe.de/api/videodb/beta/moviedata/genres",
        *        "rel": ["collection"]
        *      },
        *      "value": [{
        *        "href": null,
        *        "id": 1,
        *        "name": "Comedy"
        *      }, {
        *        "href": null,
        *        "id": 2,
        *        "name": "Romance"
        *     }, {
        *        "href": null,
        *        "id": 3,
        *        "name": "Western"
        *      }]
        *    }
        *  }
        */
        [HttpGet("genres", Name = nameof(GetAllGenres))]
        public async Task<IActionResult> GetAllGenres(CancellationToken ct)
        {
            var types = await _movieDataService.GetAllGenres(ct);

            var collection = CollectionWithPaging<MovieDataGenreResource>.Create(
             Link.ToCollection(nameof(GetAllGenres)),
             types.ToArray(),
             types.Count(),
             new PagingOptions { Limit = 100, Offset = 0 });

            return Ok(collection);
        }

        /**
       * @api {get} /moviedata/moviedata/nextdiskid/{shelterandcompartment} 4. Get next free diskid
       * @apiVersion 1.18.0
       * @apiName Get next free diskid in shelte and compartement
       * @apiGroup MoviesData
       * 
       * @apiExample Example usage:
       * http://localhost:50647/moviedata/nextdiskid/R20F3
       * 
       * @apiHeader {String} Content-Type Request type, must be "application/json".
       * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
       * @apiHeaderExample {json} Request-Example:
       * {
       *   "Content-Type": "application/json"
       *   "Authorization": "Bearer ewrjfjfoweffefo98098"
       * }
       * @apiError 401 Unauthorized
       * @apiSuccessExample Success-Response:
       *     HTTP/1.1 200 OK
       *     {
       *       R20F3D08
       *     }
       */
        [HttpGet("nextdiskid/{shelterandcompartment}", Name = nameof(GetNextFreeDiskId))]
        public async Task<IActionResult> GetNextFreeDiskId([FromRoute] string shelterandcompartment, CancellationToken ct)
        {
            return Ok(await _movieDataService.GetNextFreeDiskId(shelterandcompartment));
        }

        [HttpGet("image/cover/{MovieId}", Name = nameof(GetMovieCoverImage))]
        [OpenApiOperation("Get image for the given id", "Returns movie image as FileContentResult")]
        [SwaggerResponse(200, typeof(CollectionWithPagingAndDateFilter<MovieDataSeenResource>), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public FileContentResult GetMovieCoverImage(GetMovieByIdParameters parameters,
        CancellationToken ct)
        {
            var byteStream = _movieDataService.GetCoverImageStream(parameters.MovieId);
            return File(byteStream, "image/jpg");
        }

        [HttpGet("image/background/{MovieId}", Name = nameof(GetMovieBackgroundImage))]
        [OpenApiOperation("Get background image for the given id", "Returns movie image as FileContentResult")]
        [SwaggerResponse(200, typeof(CollectionWithPagingAndDateFilter<MovieDataSeenResource>), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public ActionResult GetMovieBackgroundImage(GetMovieByIdParameters parameters,
        CancellationToken ct)
        {
            var byteStream = _movieDataService.GetBackgroundImageStream(parameters.MovieId);
            if (byteStream != null)
            {
                return new FileContentResult(byteStream, "image/png");
            }
            else return new NotFoundResult();
        }

        [HttpGet("racks", Name = nameof(GetRacks))]
        [OpenApiOperation("Get all bays", "Return a flat list of all bays in all racks.")]
        [SwaggerResponse(200, typeof(IEnumerable<string>), Description = "-")]
        [SwaggerResponse(401, typeof(void), Description = "You need to be authorized to use this api.")]
        [ValidateModel]
        public async Task<IActionResult> GetRacks(CancellationToken ct)
        {

            var racks = await _movieDataService.GetRacks(ct);
            return Ok(racks);
        }

        private string CurrentUserName
        {
            get
            {
                var userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                return userName;
            }
        }

        private string CurrentViewGroup
        {
            get
            {
                var viewGroup = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GroupSid).Value;
                return viewGroup;
            }
        }
    }
}
