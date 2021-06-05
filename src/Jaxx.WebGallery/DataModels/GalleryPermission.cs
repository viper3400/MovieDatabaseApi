using System.ComponentModel.DataAnnotations;

namespace Jaxx.WebGallery.DataModels
{
    public class GalleryPermission
    {
        public int Id { get; set; }
        [MaxLength(256)]
        public string User { get; set; }
        public int PermissionReference { get; set; }
    }
}