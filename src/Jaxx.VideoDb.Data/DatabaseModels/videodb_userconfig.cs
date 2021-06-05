using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public class videodb_userconfig
    {
        public int user_id { get; set; }
        public string opt { get; set; }
        public string value { get; set; }
    }
}
