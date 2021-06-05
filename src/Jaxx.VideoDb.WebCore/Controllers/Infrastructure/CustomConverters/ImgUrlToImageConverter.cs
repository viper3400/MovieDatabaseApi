using AutoMapper;
using Jaxx.Images;
using Jaxx.VideoDb.Data.BusinessModels;
using Jaxx.VideoDb.Data.DatabaseModels;
using Jaxx.VideoDb.WebCore.Models;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Jaxx.VideoDb.WebCore.Controllers.Infrastructure.CustomConverters
{
    internal class ImgUrlToImageConverter : IValueResolver<videodb_videodata, MovieDataResource, byte[]>
    {
        private readonly ImageStreamer imageStreamer;
        private readonly MovieDataServiceOptions options;
        private readonly ILogger<ImgUrlToImageConverter> logger;

        public ImgUrlToImageConverter(ImageStreamer imageStreamer, MovieDataServiceOptions options, ILogger<ImgUrlToImageConverter> logger)
        {
            this.imageStreamer = imageStreamer;
            this.options = options;
            this.logger = logger;
        }
        public byte[] Resolve(videodb_videodata source, MovieDataResource destination, byte[] destMember, ResolutionContext context)
        {
            byte[] result = null;
            object useInlinceCoverImages;
            try
            {
                var value = context.Items.TryGetValue(AutoMapperConstants.INLINE_COVER_IMAGE, out useInlinceCoverImages);
                if ((bool)useInlinceCoverImages)
                {
                    result = imageStreamer.ReadImageFile(Path.Join(options.LocalCoverImagePath, source.id + ".jpg"));
                }
            }
            catch (InvalidOperationException e)
            {
                logger.LogError(e.Message);
            }

            return result;
        }
    }
}
