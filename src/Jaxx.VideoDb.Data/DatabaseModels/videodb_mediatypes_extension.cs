using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public partial class videodb_mediatypes
    {
        public videodb_mediatypes()
        {
            Videos = new List<videodb_videodata>();
        }
        public ICollection<videodb_videodata> Videos { get; set; }
    }
}
