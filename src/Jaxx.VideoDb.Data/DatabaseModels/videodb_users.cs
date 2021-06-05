using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public partial class videodb_users
    {
        public int id { get; set; }
        public string name { get; set; }
        public string passwd { get; set; }
        public string cookiecode { get; set; }
        public int permissions { get; set; }
        public string email { get; set; }
    }
}
