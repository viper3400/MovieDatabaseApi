using Jaxx.WebApi.Shared.Infrastructure;
using Jaxx.WebApi.Shared.Models;
using Jaxx.WebGallery.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaxx.WebGallery.Controllers
{
    [Route("/[controller]")]
    [OpenApiTag("GalleryAdmin", Description = "ApiController for gallery admin functions.")]
    [Authorize(Policy = "GalleryAdmin")]
    public class GalleryAdminController : Controller
    {
        private readonly IGalleryService galleryService;
        private readonly GalleryConfiguration galleryConfiguration;
        private readonly PagingOptions defaultPagingOptions;
        private readonly ILogger logger;

        public GalleryAdminController(IGalleryService galleryService, IOptions<PagingOptions> defaultPagingOptions, ILogger<GalleryAdminController> logger, GalleryConfiguration galleryConfiguration)
        {
            this.galleryService = galleryService;
            this.defaultPagingOptions = defaultPagingOptions.Value;
            this.logger = logger;
            this.galleryConfiguration = galleryConfiguration;
        }

        [HttpGet("renew-thumbs", Name = nameof(RenewThumbs))]
        [ValidateModel]
        [OpenApiOperation("Get all albums", "Returs all albums in gallery.")]
        public IActionResult RenewThumbs(CancellationToken ct)
        {
            galleryService.GenerateAlbumThumbs(galleryConfiguration.AlbumThumbPath);
            galleryService.GenerateImageThumbs(galleryConfiguration.ImageThumbPath);
            return Ok("Success");
        }
    }
}
