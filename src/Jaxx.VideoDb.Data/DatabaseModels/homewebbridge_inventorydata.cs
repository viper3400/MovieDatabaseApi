using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public class homewebbridge_inventorydata
    {
        public int id { get; set; }
        public int state { get; set; }
        public int inventoryid { get; set; }
        public int movieid { get; set; }
        public string rackid { get; set; }
        public virtual homewebbridge_inventory inventory { get; set; }
    }
}
