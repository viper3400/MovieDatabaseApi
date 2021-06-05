using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Jaxx.Images
{
    public static class ImageResizer
    {
        /// <summary>
        /// Resize the the given image and save it with the same filename into another folder. If width or height
        /// is -1, the ratio of the image will be kept in this dimension.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="thumbPath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void ResizeImage(string sourceFile, string thumbPath, int width, int height)
        {
            var fileName = Path.GetFileName(sourceFile);
            ResizeImage(sourceFile, thumbPath, width, height, fileName);
        }

        /// <summary>
        /// Resize the the given image and save it with the same filename into another folder. If width or height
        /// is -1, the ratio of the image will be kept in this dimension.
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="thumbPath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="thumbFileName"></param>
        public static void ResizeImage(string sourceFile, string thumbPath, int width, int height, string thumbFileName)
        {
            var destinationFilePath = Path.Combine(thumbPath, thumbFileName);
            using (Image image = Image.Load(sourceFile))
            {
                var size = new Size();

                if (width == -1)
                {
                    size = KeepRatioWidth(image.Width, image.Height, height);
                }
                else if (height == -1)
                {
                    size = KeepRatioHeight(image.Width, image.Height, width);
                }
                else
                {
                    size.Height = height;
                    size.Width = width;
                }

                image.Mutate(x => x.Resize(size));
                image.Save(destinationFilePath);
            };
        }

        /// <summary>
        /// Calls ResizeImage for all files in source directory matching the extensionFilter
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="extensionFilter"></param>
        /// <param name="thumbPath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void ResizeImages(string sourceDirectory, string extensionFilter, string thumbPath, int width, int height)
        {
            var images = Directory.EnumerateFiles(sourceDirectory, extensionFilter, SearchOption.TopDirectoryOnly);
            foreach (var image in images)
            {
                ResizeImage(image, thumbPath, width, height);
            }
        }

        private static Size KeepRatioWidth (int width, int height, int newHeight)
        {
                return new Size { Height = newHeight, Width = width * newHeight / height };
        }
        private static Size KeepRatioHeight(int width, int height, int newWidth)
        {
            return new Size { Height = height * newWidth / width, Width = newWidth };
        }
    }
}
