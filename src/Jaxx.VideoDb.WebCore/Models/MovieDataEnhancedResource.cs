using Jaxx.WebApi.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Models
{
    public class MovieDataEnhancedResource : Resource
    {
        public int Id { get; set; }
        public MovieDataResource MovieDataResource { get; set; }
        public List<MovieDataGenreResource> MovieDataGenres {get ;set; }
    }
}
