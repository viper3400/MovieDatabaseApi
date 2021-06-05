using Jaxx.WebApi.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Jaxx.VideoDb.WebCore.Models
{
    public class InventoryDataResource : Resource
    {
        [Required]
        public int id { get; set; }
        public int state { get; set; }
        [Required]
        public int inventoryid { get; set; }
        [Required]
        public int movieid { get; set; }
    }
}
