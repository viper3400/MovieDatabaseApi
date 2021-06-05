using Microsoft.EntityFrameworkCore;

namespace Jaxx.WebGallery.DataModels
{
    public class GalleryContext : DbContext
    {
        public GalleryContext(DbContextOptions<GalleryContext> options)
            : base(options)
        {
        }
        public DbSet<GalleryImage> GalleryImages { get; set; }
        public DbSet<GalleryAlbum> GalleryAlbums { get; set; }
        public DbSet<GalleryPermission> GalleryPermissions { get; set; }
    }
}