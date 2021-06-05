using Jaxx.WebApi.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Jaxx.WebGallery.ResourceModels
{
    public class GalleryImageResource : Resource
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public byte[] ImageBase64Enc { get; set; }
    }
}