using AutoMapper;
using Jaxx.Images;
using Jaxx.WebGallery.DataModels;
using Jaxx.WebGallery.ResourceModels;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Jaxx.VideoDb.WebCore.Controllers.Infrastructure.CustomConverters
{
    internal class ImageFileToByteConverter : IValueResolver<GalleryImage, GalleryImageResource, byte[]>
    {
        private readonly ImageStreamer imageStreamer;
        private readonly ILogger<ImageFileToByteConverter> logger;

        public ImageFileToByteConverter(ImageStreamer imageStreamer,ILogger<ImageFileToByteConverter> logger)
        {
            this.imageStreamer = imageStreamer;
            this.logger = logger;
        }
        public byte[] Resolve(GalleryImage source, GalleryImageResource destination, byte[] destMember, ResolutionContext context)
        {
            byte[] result = null;
            try
            {
                    result = imageStreamer.ReadImageFile(source.LocalPath);
            }
            catch (InvalidOperationException e)
            {
                logger.LogError(e.Message);
            }

            return result;
        }
    }
}