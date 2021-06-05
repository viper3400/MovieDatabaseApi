using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.WebCore.Models
{
    public class MovieMetaEngineOptions
    {
        [FromQuery]
        public string Engine { get; set; }
        [FromQuery]
        public string BackgroundImageEngine { get; set; }
    }
}
