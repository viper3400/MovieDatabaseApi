using AutoMapper;
using Jaxx.Images;
using Jaxx.WebApi.Shared.Models;
using Jaxx.WebGallery.DataModels;
using Jaxx.WebGallery.ResourceModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jaxx.WebGallery.Services
{
    public class DefaultGalleryService : IGalleryService
    {
        private readonly GalleryContext context;
        private readonly ILogger logger;
        private readonly IMapper mapper;
        private readonly GalleryConfiguration galleryConfiguration;
        public DefaultGalleryService(GalleryContext context, ILogger<DefaultGalleryService> logger, IMapper mapper, GalleryConfiguration galleryConfiguration)
        {
            this.context = context;
            this.logger = logger;
            this.mapper = mapper;
            this.galleryConfiguration = galleryConfiguration;
        }
        public async Task<Page<GalleryAlbumResource>> GetAlbumsAsync(PagingOptions pagingOptions, CancellationToken ct)
        {
            logger.LogDebug("Getting all albums.");
            var query = context.GalleryAlbums;
            var size = await query.CountAsync(ct);
            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ToListAsync(ct);

            var mappedItems = mapper.Map<IEnumerable<GalleryAlbumResource>>(items);

            return new Page<GalleryAlbumResource>
            {
                Items = mappedItems,
                TotalSize = size
            };

        }

        public GalleryImageResource GetGalleryImage(int imageId)
        {
            var item = context.GalleryImages
                .Where(i => i.Id == imageId)
                .FirstOrDefault();

            var mappedItem = mapper.Map<GalleryImageResource>(item);
            return mappedItem;
        }

        public async Task<Page<GalleryImageResource>> GetAlbumByIdAsync(int id, PagingOptions pagingOptions, CancellationToken ct)
        {
            var query = context.GalleryImages.Where(i => i.AlbumId == id);

            var size = await query.CountAsync(ct);
            var items = await query
               .Skip(pagingOptions.Offset.Value)
               .Take(pagingOptions.Limit.Value)
               .ToListAsync(ct);

            var mappedItems = mapper.Map<IEnumerable<GalleryImageResource>>(items);

            return new Page<GalleryImageResource>
            {
                Items = mappedItems,
                TotalSize = size
            };
        }

        public void GenerateAlbumThumbs(string thumbPath)
        {
            Directory.CreateDirectory(thumbPath);
            var albums = context.GalleryAlbums.ToList();
            foreach (var album in albums)
            {
                var images = context.GalleryImages.Where(i => i.AlbumId == album.Id).ToList();
                GalleryImage thumbImage = string.IsNullOrWhiteSpace(album.ThumbPath) ? images.FirstOrDefault() : images.Where(i => i.Id.ToString(CultureInfo.CurrentCulture) == album.ThumbPath).FirstOrDefault();
                ImageResizer.ResizeImage(thumbImage.LocalPath, thumbPath, 400, -1, $"{album.Id}.jpg");
            }
        }

        public Task DeleteAlbumById(int id, CancellationToken ct)
        {
            var entity = context.GalleryAlbums.Where(i => i.Id == id).FirstOrDefault();
            context.GalleryAlbums.Remove(entity);
            return context.SaveChangesAsync(ct);
            
        }

        public async Task<GalleryAlbumResource> CreateAlbumAsync(GalleryAlbumResource albumResource, CancellationToken ct)
        {
            var entity = mapper.Map<GalleryAlbum>(albumResource);
            context.Add(entity);
            var saved = await context.SaveChangesAsync(ct);
            return mapper.Map<GalleryAlbumResource>(entity);
            
        }

        /// <summary>
        /// Generates ImageThumbs for the given album id
        /// </summary>
        /// <param name="thumbPath"></param>
        /// <param name="albumId"></param>
        public void GenerateImageThumbs(string thumbPath, int albumId)
        {
            var images = context.GalleryImages.Where(i => i.AlbumId == albumId).ToList();
            var albumCachePath = Path.Join(thumbPath, albumId.ToString());
            Directory.CreateDirectory(albumCachePath);
            logger.LogInformation($"Generating images for album id {albumId} to path {thumbPath}");
            foreach (var image in images)
            {
                ImageResizer.ResizeImage(image.LocalPath, albumCachePath, 800, -1, $"{image.Id}.jpg");
            }
        }

        /// <summary>
        /// Generates images for all albums in the gallery.
        /// </summary>
        /// <param name="thumbPath"></param>
        public void GenerateImageThumbs(string thumbPath)
        {
            var albums = context.GalleryAlbums.ToList();
            foreach (var album in albums)
            {
                GenerateImageThumbs(thumbPath, album.Id);
            }
        }

        public IEnumerable<string> CheckDbConsistency()
        {
            var imagePath = galleryConfiguration.ImagePath;
            logger.LogInformation($"Check for album consistency, local folder is: {imagePath}");
            
            var albumDirectories = Directory.EnumerateDirectories(imagePath, "*", SearchOption.TopDirectoryOnly);
            var dbAlbums = context.GalleryAlbums.Select(a => a.LocalPath).ToList();

            var resultList = new List<string>();

            var differenceMissingInDb = albumDirectories.Except(dbAlbums);
            foreach (var d in differenceMissingInDb)
            {
                var message = $"Gallery Album on Db not found {d})";
                logger.LogError(message);
                resultList.Add(message);
            }

            var differenceMissingFolder = dbAlbums.Except(albumDirectories);
            foreach (var d in differenceMissingFolder)
            {
                var message = $"Folder not found: {d})";
                logger.LogError(message);
                resultList.Add(message);
            }

            return resultList;
        }
    }
}
