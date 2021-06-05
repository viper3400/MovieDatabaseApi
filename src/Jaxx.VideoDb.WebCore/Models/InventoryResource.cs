using Jaxx.WebApi.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Jaxx.VideoDb.WebCore.Models
{
    public class InventoryResource : Resource
    {
        [Required]
        public int id { get; set; }
        public DateTime? starttime { get; set; }
        public DateTime? endtime { get; set; }
        public string name { get; set; }
        public int state { get; set; }
    }
}
