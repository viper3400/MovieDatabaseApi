using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public partial class homewebbridge_userseen
    {
        public virtual videodb_videodata MovieInformation { get; set; }
    }
}
