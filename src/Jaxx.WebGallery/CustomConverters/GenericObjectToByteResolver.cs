using AutoMapper;
using Jaxx.Images;
using Jaxx.WebGallery.DataModels;
using Jaxx.WebGallery.ResourceModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Jaxx.WebGallery.CustomConverters
{
    internal class GenericObjectToByteResolver : IValueResolver<object, object, byte[]>
    {
        private readonly ImageStreamer imageStreamer;
        private readonly ILogger logger;
        private readonly GalleryConfiguration galleryConfiguration;

        public GenericObjectToByteResolver(ImageStreamer imageStreamer, ILogger<GenericObjectToByteResolver> logger, GalleryConfiguration galleryConfiguration)
        {
            this.imageStreamer = imageStreamer;
            this.logger = logger;
            this.galleryConfiguration = galleryConfiguration;
        }
        public byte[] Resolve(object source, object destination, byte[] destMember, ResolutionContext context)
        {
            string streamingPath;

            if (source.GetType() == typeof(GalleryAlbum))
            {
                var galleryAlbum = source as GalleryAlbum;
                string albumThumbPath = galleryConfiguration.AlbumThumbPath;
                streamingPath = Path.Combine(albumThumbPath, $"{galleryAlbum.Id}.jpg");
            }
            else if (source.GetType() == typeof(GalleryImage))
            {
                var galleryImage = source as GalleryImage;
                string imageThumbPath = galleryConfiguration.ImageThumbPath;
                streamingPath = Path.Combine(imageThumbPath, galleryImage.AlbumId.ToString(CultureInfo.CurrentCulture), $"{galleryImage.Id}.jpg");
            }
            else throw new NotSupportedException($"Source type {source.GetType()} not supported.");

            byte[] result = null;
            try
            {
                result = imageStreamer.ReadImageFile(streamingPath);
            }
            catch (InvalidOperationException e)
            {
                logger.LogError(e.Message);
            }

            return result;
        }
    }
}
