using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jaxx.WebGallery.DataModels
{
    public class GalleryImage
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public string Name { get; set; }
        public string LocalPath { get; set; }
        public string RemotePath { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string Permissions { get; set; }
    }
}