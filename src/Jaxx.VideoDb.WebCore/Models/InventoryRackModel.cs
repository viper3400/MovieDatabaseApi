using Jaxx.VideoDb.WebCore.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.WebCore.Models
{
    public class InventoryRackModel
    {
        public string RackId { get; set; }
        public InventoryDataState RackState { get; set; }
        public InventoryState RackInventoryState { get; set; }
    }
}
