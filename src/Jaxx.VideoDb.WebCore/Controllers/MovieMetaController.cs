using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.WebCore.Infrastructure;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.VideoDb.WebCore.Services;
using Jaxx.WebApi.Shared.Infrastructure;
using Jaxx.WebApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jaxx.VideoDb.WebCore.Controllers
{
    [Route("/[controller]")]
    [Authorize(Policy = "VideoDbUser")]
    public class MovieMetaController : Controller
    {
        private readonly PagingOptions _defaultPagingOptions;
        private readonly MovieMetaEngineOptions _defaultMovieMetaEngineOptions;
        private readonly ILogger _logger;
        private readonly IMovieMetaService _metaService;        

        public MovieMetaController(
         IMovieMetaService movieMetaService,
         IOptions<PagingOptions> defaultPagingOptionsAccessor,
         IOptions<MovieMetaEngineOptions> defaultMovieMetaEngineAccessor,
         ILogger<MovieDataController> logger)
        {
            _metaService = movieMetaService;
            _defaultPagingOptions = defaultPagingOptionsAccessor.Value;
            _defaultMovieMetaEngineOptions = defaultMovieMetaEngineAccessor.Value;
            _logger = logger;
        }

        /**
        * @api {get} /moviemeta/searchtitle/:title 2. Get By Movie Title
        * @apiVersion 1.17.0
        * @apiName GetMovieMetaByTitle
        * @apiGroup MovieMeta
        * 
        * @apiDescription Get movie meta data by title.
        * 
        * @apiParam {Number} [limit] Paging option. Maximum number of returned stakeholders per page.
        * @apiParam {Number} [offset] Paging option. The offset from where the API will start to return stakeholders.
        *                              Must be greater than 0.
        * @apiParam {String} [engine] Ofdb or TheMovieDb, Ofdb is default
        *
        * @apiExample Example usage:
        * http://localhost:50647/moviemeta/searchtitle/Batman
        * http://localhost:50647/moviemeta/searchtitle/Batman?engine=TheMovieDb
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
        *  {
        *       ! Result same as "Get By Movie Reference"
        *       ! CAVEAT: No actors and no edtions will be returned.
        *       ! These informations are only provide when searching by reference.
        *  }
        **/
        [HttpGet("searchtitle/{title}", Name = nameof(GetMovieMetaAsync))]
        [ValidateModel]
        public async Task<IActionResult> GetMovieMetaAsync(
         [FromQuery] PagingOptions pagingOptions,
         [FromQuery] MovieMetaEngineOptions engineOptions,
         [FromRoute(Name = "title")] string title,
         CancellationToken ct)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            engineOptions.Engine = engineOptions.Engine ?? _defaultMovieMetaEngineOptions.Engine;            
            _metaService.ChangeEngineType(engineOptions.Engine);

            var movies = await _metaService.SearchMovieByTitleAsync(title, pagingOptions, ct);

            var collection = CollectionWithPaging<MovieMetaResource>.Create(
                Link.ToCollection(nameof(GetMovieMetaAsync), title),
                movies.Items.ToArray(),
                movies.TotalSize,
                pagingOptions);

            return Ok(collection);

        }

        /**
        * @api {get} /moviemeta/:reference 1. Get By Movie Reference
        * @apiVersion 1.5.0
        * @apiName GetMovieMetaByReference
        * @apiGroup MovieMeta
        * 
        * @apiDescription Get movie meta data reference.
        * 
        * @apiParam {Number} [limit] Paging option. Maximum number of returned stakeholders per page.
        * @apiParam {Number} [offset] Paging option. The offset from where the API will start to return stakeholders.
        *                              Must be greater than 0.
        * @apiExample Example usage:
        * http://localhost:50647/moviemeta/277170
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
        *
        * {
        *   "href": "https://danielgraefe.de/api/videodb/beta/moviemeta/277170?Length=6",
        *   "rel": ["collection"],
        *   "offset": 0,
        *   "limit": 25,
        *   "size": 1,
        *   "first": {
        *     "href": "https://danielgraefe.de/api/videodb/beta/moviemeta/277170?Length=6",
        *     "rel": ["collection"]
        *   },
        *   "value": [{
        *     "href": null,
        *     "metaEngine": "ofdb",
        *     "reference": "277170",
        *     "title": "Kirschblüten und rote Bohnen",
        *     "subTitle": null,
        *     "originalTitle": null,
        *     "year": "2015",
        *     "productionCountry": "Deutschland",
        *     "imgUrl": "https://ssl.ofdb.de/images/film/277/277170.jpg",
        *     "length": null,
        *     "barcode": null,
        *     "actors": [{
        *       "metaEngine": "ofdb",
        *       "reference": null,
        *       "actorName": "Kirin Kiki"
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": null,
        *       "actorName": "Masatoshi Nagase"
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": null,
        *       "actorName": "Kyara Uchida"
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": null,
        *       "actorName": "Etsuko Ichihara"
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": null,
        *       "actorName": "Miyoko Asada"
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": null,
        *       "actorName": "Wakato Kanematsu"
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": null,
        *       "actorName": "Miki Mizuno"
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": null,
        *       "actorName": "Yuria Murata"
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": null,
        *       "actorName": "Taiga"
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": null,
        *       "actorName": "Saki Takahashi"
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": null,
        *       "actorName": "Miu Takeuchi"
        *     }],
        *     "plot": null,
        *     "rating": "6.72",
        *     "editions": [{
        *       "metaEngine": "ofdb",
        *       "reference": "277170;434024",
        *       "name": "DVD: good!movies",
        *       "country": "Deutschland",
        *       "length": null
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": "277170;416634",
        *       "name": "Blu-ray Disc: good!movies",
        *       "country": "Deutschland",
        *       "length": null
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": "277170;408229",
        *       "name": "Kino: Neue Visionen",
        *       "country": "Deutschland",
        *       "length": null
        *     }, {
        *       "metaEngine": "ofdb",
        *       "reference": "277170;436144",
        *       "name": "Free-TV: arte HD",
        *       "country": "Deutschland",
        *       "length": null
        *     }],
        *     "genres": ["Drama"]
        *   }]
        * }
        */
        [HttpGet("{reference}", Name = nameof(GetMovieMetaAsyncById))]
        [ValidateModel]
        public async Task<IActionResult> GetMovieMetaAsyncById(
         [FromQuery] PagingOptions pagingOptions,
         [FromRoute(Name = "reference")] string reference,
         CancellationToken ct)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;

            var movies = await _metaService.SearchMovieByIdAsync(reference, pagingOptions, ct);

            var collection = CollectionWithPaging<MovieMetaResource>.Create(
                Link.ToCollection(nameof(GetMovieMetaAsyncById), reference),
                movies.Items.ToArray(),
                movies.TotalSize,
                pagingOptions);

            return Ok(collection);

        }

        /**
        * @api {get} /moviemeta/searchbarcode/:barcode 3. Get By Movie Barcode
        * @apiVersion 1.5.0
        * @apiName GetMovieMetaByBarcode
        * @apiGroup MovieMeta
        * 
        * @apiDescription Get movie meta data by barcode.
        * 
        * @apiParam {Number} [limit] Paging option. Maximum number of returned stakeholders per page.
        * @apiParam {Number} [offset] Paging option. The offset from where the API will start to return stakeholders.
        *                              Must be greater than 0.
        * @apiExample Example usage:
        * http://localhost:50647/moviemeta/searchbarcode/5845121712
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
        *  {
        *       Result same as "Get By Movie Reference"
        *  }
        **/
        [HttpGet("searchbarcode/{barcode}", Name = nameof(GetMovieMetaByBarcodeAsync))]
        [ValidateModel]
        public async Task<IActionResult> GetMovieMetaByBarcodeAsync(
         [FromQuery] PagingOptions pagingOptions,
         [FromRoute(Name = "barcode")] string barcode,
         CancellationToken ct)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? _defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? _defaultPagingOptions.Limit;
            
            var movies = await _metaService.SearchMovieByBarcodeAsync(barcode, pagingOptions, ct);

            var collection = CollectionWithPaging<MovieMetaResource>.Create(
                Link.ToCollection(nameof(GetMovieMetaByBarcodeAsync), barcode),
                movies.Items.ToArray(),
                movies.TotalSize,
                pagingOptions);

            return Ok(collection);

        }

        /**
        * @api {POST} /moviemeta/convert 4. Convert Movie Meta
        * @apiVersion 1.13.0
        * @apiName ConvertMovieMeta
        * @apiGroup MovieMeta
        * 
        * @apiDescription Converts a MovieMetaResource to MovieDataResource
        *     
        * @apiExample Example usage:
        * http://localhost:50647/moviemeta/convert
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        * @apiParam {String} json A single MovieMetaResource json string             
        * 
        * @apiParamExample {json} Request-Example:
        * {
        *           
        * }        
        * 
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:        
        *     HTTP/1.1 200 OK
        *  {
        *       see GetByMovieReference, json could be a single "value" of this result
        *  }
        **/
        [HttpPost("convert", Name = nameof(ConvertMovieMetaAsync))]
        [ValidateModel]
        public async Task<IActionResult> ConvertMovieMetaAsync(
         [FromBody] MovieMetaResource movieMetaResource,
         CancellationToken ct)
        {

            var convertedMovie = await _metaService.ConvertMovieMetaAsync(movieMetaResource, ct);
            return Ok(convertedMovie);

        }
    }
}