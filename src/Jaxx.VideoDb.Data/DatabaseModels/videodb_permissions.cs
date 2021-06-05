using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public class videodb_permissions
    {
        public int from_uid { get; set; }
        public int to_uid { get; set; }
        public int permissions { get; set; }
    }
}
