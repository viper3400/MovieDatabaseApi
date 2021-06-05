using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.Data.BusinessModels
{
    public class MovieMetaEngineDefinition
    {
        public MovieMetaEngineType TypeName { get; set; }
        public string TypeAccessor { get; set; }
        public string FriendlyName { get; set; }
    }
}
