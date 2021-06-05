using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Controllers
{
    public class GetMovieByIdParameters
    {
        [FromRoute]
        public int MovieId { get; set; }
    }
}
