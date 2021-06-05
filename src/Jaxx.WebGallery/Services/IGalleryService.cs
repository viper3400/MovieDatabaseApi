using Jaxx.WebApi.Shared.Models;
using Jaxx.WebGallery.ResourceModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Jaxx.WebGallery.Services
{
    public interface IGalleryService
    {
        Task<Page<GalleryAlbumResource>> GetAlbumsAsync(PagingOptions pagingOptions, CancellationToken ct);
        GalleryImageResource GetGalleryImage(int imageId);
        Task<Page<GalleryImageResource>> GetAlbumByIdAsync(int id, PagingOptions pagingOptions, CancellationToken ct);

        /// <summary>
        /// Generates images for all albums in the gallery.
        /// </summary>
        /// <param name="thumbPath"></param>
        void GenerateImageThumbs(string thumbPath);

        /// <summary>
        /// Generates ImageThumbs for the given album id
        /// </summary>
        /// <param name="thumbPath"></param>
        /// <param name="albumId"></param>
        void GenerateImageThumbs(string thumbPath, int albumId);
        void GenerateAlbumThumbs(string thumbPath);
        IEnumerable<string> CheckDbConsistency();
        Task<GalleryAlbumResource> CreateAlbumAsync(GalleryAlbumResource albumResource, CancellationToken ct);
        Task DeleteAlbumById(int id, CancellationToken ct);
    }
}