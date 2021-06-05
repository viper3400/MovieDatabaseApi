using System.Threading;
using System.Threading.Tasks;
using Jaxx.VideoDb.WebCore.Models;
using Jaxx.WebApi.Shared.Models;

namespace Jaxx.VideoDb.WebCore.Services
{
    public interface IMovieMetaService
    {
        Task<Page<MovieMetaResource>> SearchMovieByTitleAsync(string title, PagingOptions pagingOptions, CancellationToken ct);
        Task<Page<MovieMetaResource>> SearchMovieByIdAsync(string reference, PagingOptions pagingOptions, CancellationToken ct);
        Task<Page<MovieMetaResource>> SearchMovieByBarcodeAsync(string barcode, PagingOptions pagingOptions, CancellationToken ct);
        Task<MovieDataResource> ConvertMovieMetaAsync(MovieMetaResource movieMetaResource, CancellationToken ct);
        void ChangeEngineType(string engine);
    }
}