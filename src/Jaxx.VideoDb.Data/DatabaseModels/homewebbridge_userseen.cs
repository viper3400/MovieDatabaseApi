using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public partial class homewebbridge_userseen
    {
        public int id { get; set; }
        public int vdb_videoid { get; set; }
        public DateTime viewdate { get; set; }
        public string asp_viewgroup { get; set; }
        public string asp_username { get; set; }
    }
}
