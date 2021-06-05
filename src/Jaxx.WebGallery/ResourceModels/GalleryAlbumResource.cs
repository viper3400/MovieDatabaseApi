using Jaxx.WebApi.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Jaxx.WebGallery.ResourceModels
{
    public class GalleryAlbumResource : Resource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LocalPath { get; set; }
        public string FirstDate { get; set; }
        public string LastDate { get; set; }
        public string Description { get; set; }
        [MaxLength(256)]
        public string ThumbPath { get; set; }
        public byte[] ThumbImageBase64Enc { get; set; }
    }
    
}