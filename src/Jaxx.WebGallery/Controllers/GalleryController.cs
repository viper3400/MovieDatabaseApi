using Jaxx.WebApi.Shared.Controllers.Infrastructure;
using Jaxx.WebApi.Shared.Infrastructure;
using Jaxx.WebApi.Shared.Models;
using Jaxx.WebGallery.ResourceModels;
using Jaxx.WebGallery.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Namotion.Reflection;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaxx.WebGallery.Controllers
{
    [Route("/[controller]")]
    [OpenApiTag("Gallery", Description = "ApiController for gallery functions.")]
    [Authorize(Policy = "GalleryUser")]
    public class GalleryController : Controller
    {
        private readonly IGalleryService galleryService;
        private readonly PagingOptions defaultPagingOptions;
        private readonly ILogger logger;

        public GalleryController(IGalleryService galleryService, IOptions<PagingOptions> defaultPagingOptions, ILogger<GalleryController> logger)
        {
            this.galleryService = galleryService;
            this.defaultPagingOptions = defaultPagingOptions.Value;
            this.logger = logger;
        }

        [HttpGet("album", Name = nameof(GetAlbumAsync))]
        [ValidateModel]
        [OpenApiOperation("Get all albums", "Returs all albums in gallery.")]
        public async Task<IActionResult> GetAlbumAsync([FromQuery] PagingOptions pagingOptions, CancellationToken ct)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? defaultPagingOptions.Limit;

            var albums = await galleryService.GetAlbumsAsync(pagingOptions, ct);

            var collection = CollectionWithPaging<GalleryAlbumResource>.Create(Link.ToCollection(nameof(GetAlbumAsync)), albums.Items.ToArray(), albums.TotalSize, pagingOptions);

            return Ok(collection);
        }

        [HttpGet("album/{Id}", Name = nameof(GetAlbumByIdAsync))]
        [ValidateModel]
        [OpenApiOperation("Get all images from album.", "Get all images from album.")]
        public async Task<IActionResult> GetAlbumByIdAsync(GetByGenericIdParameter parameter, [FromQuery] PagingOptions pagingOptions, CancellationToken ct)
        {
            pagingOptions.Offset = pagingOptions.Offset ?? defaultPagingOptions.Offset;
            pagingOptions.Limit = pagingOptions.Limit ?? defaultPagingOptions.Limit;

            var album  = await galleryService.GetAlbumByIdAsync(parameter.Id, pagingOptions, ct);

            var collection = CollectionWithPaging<GalleryImageResource>.Create(
            Link.ToCollection(nameof(GetAlbumByIdAsync)),
            album.Items.ToArray(),
            album.TotalSize,
            pagingOptions);

            return Ok(collection);
        }

        [HttpGet("image/{Id}", Name = nameof(GetStreamingImage))]
        [ValidateModel]
        [OpenApiOperation("Get image", "Returs a json containig a base64enc image.")]
        public IActionResult GetStreamingImage(GetByGenericIdParameter parameter)
        {
            var image = galleryService.GetGalleryImage(parameter.Id);
            return Ok(image);
        }
    }
}
