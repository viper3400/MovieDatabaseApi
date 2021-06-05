using Jaxx.WebApi.Shared.Models;
using Jaxx.WebGallery.ResourceModels;
using Jaxx.WebGallery.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace Jaxx.WebGallery.Test
{
    public class DefaultGalleryServiceShould
    {
        private DefaultGalleryService galleryService;
        private GalleryConfiguration galleryConfiguration;

        public DefaultGalleryServiceShould()
        {
            var host = DefaultTestHost.Host().Build();
            host.StartAsync().Wait();
            galleryService = (DefaultGalleryService)host.Services.GetService(typeof(IGalleryService));
            galleryConfiguration = (GalleryConfiguration)host.Services.GetService(typeof(GalleryConfiguration));

        }
        [Fact]
        public async void GetAllAlbums()
        {
            var pagingOptions = new PagingOptions { Limit = 25, Offset = 0 };
            var ct = new CancellationToken();
            var albums = await galleryService.GetAlbumsAsync(pagingOptions, ct);
            Assert.Equal(23, albums.TotalSize);
        }

        [Fact]
        public async void GetAlbumByIdAsync()
        {
            var pagingOptions = new PagingOptions { Limit = 50, Offset = 0 };
            var ct = new CancellationToken();
            var album = await galleryService.GetAlbumByIdAsync(10, pagingOptions, ct);
            Assert.Equal(24, album.TotalSize);
        }

        [Fact]
        public void GetGalleryImage()
        {
            var image = galleryService.GetGalleryImage(144);
            Assert.Equal("imm-050", image.Name);
        }

        [Fact]
        public void GenerateAlbumThumbs()
        {
            var path = galleryConfiguration.AlbumThumbPath;
            TestHelpers.PrepareDestinationPath(path);
            galleryService.GenerateAlbumThumbs(path);
            var thumbFileCount = System.IO.Directory.EnumerateFiles(path).Count();
            Assert.Equal(23, thumbFileCount);
            TestHelpers.ClearDestinationPath(path);
        }

        /// <summary>
        /// We create thumb images for album #3.
        /// We expect 92 files in .\{ImageThumbPath}\3 (3 as subfoder for album #3)
        /// </summary>
        [Fact]
        public void GenerateImageThumbs()
        {
            var path = galleryConfiguration.ImageThumbPath;
            var albumCachePath = System.IO.Path.Join(path, "3");
            TestHelpers.PrepareDestinationPath(albumCachePath);
            galleryService.GenerateImageThumbs(path, 3);
            var thumbFileCount = System.IO.Directory.EnumerateFiles(albumCachePath).Count();
            Assert.Equal(92, thumbFileCount);
            TestHelpers.ClearDestinationPath(path);
        }

        [Fact]
        public async void CreateAlbum()
        {
            var ct = new CancellationToken();

            var albumResource = new GalleryAlbumResource { Name = "Test-Album", LocalPath = "testalbum" };
            var album = await galleryService.CreateAlbumAsync(albumResource, ct);
            var pagingOptions = new PagingOptions { Limit = 500, Offset = 0 };
            
            var retrieveAlbum = await galleryService.GetAlbumsAsync(pagingOptions, ct);
            Assert.Single(retrieveAlbum.Items.Where(i => i.Id == album.Id));
            Assert.Equal("Test-Album", retrieveAlbum.Items.Where(i => i.Id == album.Id).FirstOrDefault().Name);

            await galleryService.DeleteAlbumById(album.Id, ct);
            retrieveAlbum = await galleryService.GetAlbumsAsync(pagingOptions, ct);
            Assert.Empty(retrieveAlbum.Items.Where(i => i.Id == album.Id));
        }
    }
}
