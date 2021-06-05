using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieMetaEngine;
using Jaxx.VideoDb.WebCore.Models;
using System.Threading;
using AutoMapper;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.WebApi.Shared.Models;

namespace Jaxx.VideoDb.WebCore.Services
{
    public class DefaultMovieMetaService : IMovieMetaService
    {
        private IMovieMetaSearch _movieMetaSearch;
        private IEnumerable<IMovieMetaSearch> _movieMetaSearches;
        private ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IMovieMetaEngineRepository _engineRepository;
        public DefaultMovieMetaService(ILogger<DefaultMovieMetaService> logger, IMapper mapper, IEnumerable<IMovieMetaSearch> movieMetaSearches, IMovieMetaEngineRepository engineRepository)
        {
            _movieMetaSearch = movieMetaSearches.FirstOrDefault();
            _movieMetaSearches = movieMetaSearches;
            _logger = logger;
            _mapper = mapper;
            _engineRepository = engineRepository;
        }

        /// <summary>
        /// Method that generates a page result for a given list of movie meta data
        /// </summary>
        /// <param name="pagingOptions"></param>
        /// <param name="queryResult"></param>
        /// <returns></returns>
        private Page<MovieMetaResource> MetaResultToPage(PagingOptions pagingOptions, List<MovieMetaMovieModel> queryResult)
        {
            long size = queryResult.Count();
            var metaResult = queryResult
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ToList();

            var metaResultRessource = _mapper.Map<IEnumerable<MovieMetaResource>>(metaResult);

            return new Page<MovieMetaResource>
            {
                Items = metaResultRessource,
                TotalSize = size
            };
        }

        /// <summary>
        /// Searches meta information for movie by movie title
        /// </summary>
        /// <param name="title"></param>
        /// <param name="pagingOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Page<MovieMetaResource>> SearchMovieByTitleAsync (string title, PagingOptions pagingOptions, CancellationToken ct)
        {
            _logger.LogTrace("SearchMovieByTitleAsync for {0} with {1}", title, _movieMetaSearch.GetType().FullName);

            List<MovieMetaMovieModel> queryResult = new List<MovieMetaMovieModel>();

            try
            {
                queryResult = await Task.FromResult(_movieMetaSearch.SearchMovieByTitle(title));
            }
            catch (Exception ex)
            {
                _logger.LogError("SearchMovieByTitleAsync: {0}", ex.Message);
                _logger.LogError("SearchMovieByTitleAsyncStack: {0}", ex);
            }

            return MetaResultToPage(pagingOptions, queryResult);
        }       

        /// <summary>
        /// Searches meta information for movie by a reference
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="pagingOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Page<MovieMetaResource>> SearchMovieByIdAsync(string reference, PagingOptions pagingOptions, CancellationToken ct)
        {
            var queryTask = Task.Factory.StartNew(() => _movieMetaSearch.SearchMovieByEngineId(reference));
            var queryResult = await queryTask;
            return MetaResultToPage(pagingOptions, queryResult);
        }

        /// <summary>
        /// Searches meta information for movie by a barcode
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="pagingOptions"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Page<MovieMetaResource>> SearchMovieByBarcodeAsync(string barcode, PagingOptions pagingOptions, CancellationToken ct)
        {
            var queryTask = Task.Factory.StartNew(() => _movieMetaSearch.SearchMovieByBarcode(barcode));
            var queryResult = await queryTask;
            return MetaResultToPage(pagingOptions, queryResult);
        }

        public async Task<MovieDataResource> ConvertMovieMetaAsync(MovieMetaResource movieMetaResource, CancellationToken ct)
        {
            var movieDataResource = await Task.Run(() => _mapper.Map<MovieDataResource>(movieMetaResource));
            return movieDataResource;
        }

        public void ChangeEngineType(string engine)
        {
            var selectedEngine = _engineRepository.MovieMetaEngines.FirstOrDefault(e => e.FriendlyName == engine);
            _movieMetaSearch = _movieMetaSearches.FirstOrDefault(s => s.GetType().ToString() == selectedEngine.TypeAccessor);
        }
    }
}
