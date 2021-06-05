using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Jaxx.WebGallery.DataModels
{
    public class GalleryAlbum
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LocalPath { get; set; }
        public string FirstDate { get; set; }
        public string LastDate { get; set; }
        public string Description { get; set; }
        [MaxLength(256)]
        public string ThumbPath { get; set; }
             
    }
    
}