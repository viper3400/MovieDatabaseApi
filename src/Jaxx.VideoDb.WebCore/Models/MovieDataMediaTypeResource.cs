using Jaxx.WebApi.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Models
{
    public class MovieDataMediaTypeResource : Resource
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
