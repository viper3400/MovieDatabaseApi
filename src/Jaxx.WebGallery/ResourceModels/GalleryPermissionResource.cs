using Jaxx.WebApi.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace Jaxx.WebGallery.ResourceModels
{
    public class GalleryPermissionResource : Resource
    {
        public int Id { get; set; }
        [MaxLength(256)]
        public string User { get; set; }
        public int PermissionReference { get; set; }
    }
}