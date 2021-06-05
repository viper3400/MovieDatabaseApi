using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public class videodb_userseen
    {
        public int video_id { get; set; }
        public int user_id { get; set; }
    }
}
