using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public partial class videodb_videodata
    {
        public virtual videodb_users VideoOwner { get; set; }
        public virtual IEnumerable<videodb_videogenre> VideoGenres { get; set; }
        public virtual IEnumerable<homewebbridge_userseen> SeenInformation { get; set; }
        public virtual IEnumerable<homewebbridge_usermoviesettings> UserSettings { get; set; }
        public virtual videodb_mediatypes VideoMediaType { get; set; }
    }
}
