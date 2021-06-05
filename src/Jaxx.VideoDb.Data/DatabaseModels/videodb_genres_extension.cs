using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public partial class videodb_genres
    {
        public virtual IEnumerable<videodb_videogenre> VideosForGenre { get; set; }
    }
}
