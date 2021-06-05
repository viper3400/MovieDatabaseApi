using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.WebCore.Models
{
    public sealed class MovieDataOptions
    {
        [FromQuery]        
        public string Diskid { get; set; }

        [FromQuery]
        public string Title { get; set; }
        
        [FromQuery]
        public string Search { get; set; }
        
        [FromQuery]
        public string MediaTypes { get; set; }

        [FromQuery]
        public string Genres { get; set; }

        [FromQuery]
        public string IsTv { get; set; }

        [FromQuery]
        public string IsDeleted { get; set; }

        [FromQuery]
        public string NotSeen { get; set; }

        [FromQuery]
        public MovieDataSortOrder SortOrder { get; set; }

        /// <summary>
        /// Is true, API returns MovieDataRessource with base64 encoded cover image
        /// </summary>
        [FromQuery]
        public bool UseInlineCoverImage { get; set; }
    }
}
