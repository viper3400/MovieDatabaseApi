using Jaxx.WebApi.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Models
{
    public class MovieDataSeenResource : Resource
    {
        public DateTime SeenDate { get; set; }
        public MovieDataResource Movie { get; set; }
    }
}
