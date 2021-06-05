using Jaxx.Images;
using System.IO;
using Xunit;
using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations;

namespace Jaxx.WebGallery.Test
{
    public class ImageResizerShould
    {
        [Fact]
        public void ResizeImage()
        {
            var assetsPath = "./assets";
            var imageFile = "test_image01.jpg";
            var thumbsPath = "./thumbs";
            TestHelpers.PrepareDestinationPath(thumbsPath);
            ImageResizer.ResizeImage(Path.Join(assetsPath,imageFile), thumbsPath, 800, 300);

            var actualImageSize = Image.Load(Path.Join(thumbsPath, imageFile)).Size();
            Assert.Equal(800, actualImageSize.Width);
            Assert.Equal(300, actualImageSize.Height);
            TestHelpers.ClearDestinationPath(thumbsPath);
        }

        [Fact]
        public void ResizeImageWithChangedName()
        {
            var assetsPath = "./assets";
            var imageFile = "test_image01.jpg";
            var thumbFileName = "01.jpg";
            var thumbsPath = "./thumbs4";
            TestHelpers.PrepareDestinationPath(thumbsPath);
            ImageResizer.ResizeImage(Path.Join(assetsPath, imageFile), thumbsPath, 800, 300, thumbFileName);

            var actualImageSize = Image.Load(Path.Join(thumbsPath, thumbFileName)).Size();
            Assert.Equal(800, actualImageSize.Width);
            Assert.Equal(300, actualImageSize.Height);
            TestHelpers.ClearDestinationPath(thumbsPath);
        }

        [Fact]
        public void ResizeImageKeepHeightRatio()
        {
            var assetsPath = "./assets";
            var imageFile = "test_image01.jpg";
            var thumbsPath = "./thumbs2";
            TestHelpers.PrepareDestinationPath(thumbsPath);
            ImageResizer.ResizeImage(Path.Join(assetsPath, imageFile), thumbsPath, 800, -1);

            var actualImageSize = Image.Load(Path.Join(thumbsPath, imageFile)).Size();
            Assert.Equal(800, actualImageSize.Width);
            Assert.Equal(600, actualImageSize.Height);
            TestHelpers.ClearDestinationPath(thumbsPath);
        }

        [Fact]
        public void ResizeImageKeepHeighWidth()
        {
            var assetsPath = "./assets";
            var imageFile = "test_image01.jpg";
            var thumbsPath = "./thumbs3";
            TestHelpers.PrepareDestinationPath(thumbsPath);
            ImageResizer.ResizeImage(Path.Join(assetsPath, imageFile), thumbsPath, -1, 600);

            var actualImageSize = Image.Load(Path.Join(thumbsPath, imageFile)).Size();
            Assert.Equal(800, actualImageSize.Width);
            Assert.Equal(600, actualImageSize.Height);
            TestHelpers.ClearDestinationPath(thumbsPath);
        }
    }
}
