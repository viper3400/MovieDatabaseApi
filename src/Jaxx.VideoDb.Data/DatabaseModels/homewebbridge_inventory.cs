using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public class homewebbridge_inventory
    {
        public int id { get; set; }
        public DateTime? starttime { get; set; }
        public DateTime? endtime { get; set; }
        public string name { get; set; }
        public int state { get; set; }
        //public virtual IEnumerable<homewebbridge_inventorydata> data { get; set; }
    }
}
