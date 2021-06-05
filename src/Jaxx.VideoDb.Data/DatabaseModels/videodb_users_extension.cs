using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public partial class videodb_users
    {
        public videodb_users()
        {
            UserVideos = new List<videodb_videodata>();
        }

        public virtual ICollection<videodb_videodata> UserVideos { get; set; }
    }
}
