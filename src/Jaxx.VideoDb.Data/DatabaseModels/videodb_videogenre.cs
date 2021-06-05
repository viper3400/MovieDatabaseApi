using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public partial class videodb_videogenre
    {
        public int video_id { get; set; }
        public int genre_id { get; set; }
    }
}
