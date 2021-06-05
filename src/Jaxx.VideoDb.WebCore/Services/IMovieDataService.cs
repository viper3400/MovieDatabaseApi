using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.WebApi.Shared.Models;

namespace Jaxx.VideoDb.WebCore.Services
{
    public interface IMovieDataService
    {
        Task<MovieDataResource> GetMovieDataAsync(int id, CancellationToken ct);

        Task<Page<MovieDataResource>> GetMovieDataAsync(
           List<int> movieId,
           PagingOptions pagingOptions,
           MovieDataOptions movieDataOptions,
           CancellationToken ct);

        Task<Page<MovieDataResource>> GetMovieDataSurpriseAsync(int surpriseCount, MovieDataOptions movieDataOptions, CancellationToken ct);           

        Task<Page<MovieDataResource>> GetFavoriteMoviesAsync(
          string userName,
          PagingOptions pagingOptions,          
          CancellationToken ct);

        Task<Page<MovieDataResource>> GetWatchAgainMoviesAsync(
         string userName,
         PagingOptions pagingOptions,
         CancellationToken ct);

        Task<Page<MovieDataEnhancedResource>> GetMovieDataEnhancedAsync(
          int? movieId,
          PagingOptions pagingOptions,
          MovieDataOptions movieDataOptions,
          CancellationToken ct);
        Task<Page<MovieDataSeenResource>> GetSeenMovies(PagingOptions pagingOptions, DateRangeFilterOptions dateRangeFilteroption, CancellationToken cancellationToken);

        Task<MovieDataResource> CreateMovieDataAsync(MovieDataResource item, CancellationToken ct);

        Task<bool> DeleteMovieDataAsync(int id, CancellationToken cancellationToken);

        Task<MovieDataResource> UpdateMovieDataAsync(int id, MovieDataResource movieDataResource, CancellationToken cancellationToken);

        Task<Tuple<int, string>> MovieSeenSetAsync(int id, string userName, string viewGroup, DateTime date);

        Task<Tuple<int, string>> MovieSeenDeleteAsync(int id, string viewGroup, DateTime date);

        Task<MovieDataResource> SetUnsetMovieUserFlagged(int movieId, string userName, int isFlagged, CancellationToken ct);

        Task<MovieDataResource> SetUnsetMovieUserFavorite(int movieId, string userName, int isFlagged, CancellationToken ct);

        Task<IEnumerable<MovieDataMediaTypeResource>> GetAllMediaTypes(CancellationToken ct);

        Task<IEnumerable<MovieDataGenreResource>> GetAllGenres(CancellationToken ct);

        Task<string> GetNextFreeDiskId(string ShelterAndCompartement);
        Task<IEnumerable<string>> GetRacks(CancellationToken ct);
        Task DonwloadMissingImages(CancellationToken ct, int sleepTime = 0);
        byte[] GetCoverImageStream(int id);
        byte[] GetBackgroundImageStream(int id);
    }
}
