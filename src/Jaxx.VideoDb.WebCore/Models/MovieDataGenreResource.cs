using Jaxx.WebApi.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Models
{
    public class MovieDataGenreResource : Resource
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
