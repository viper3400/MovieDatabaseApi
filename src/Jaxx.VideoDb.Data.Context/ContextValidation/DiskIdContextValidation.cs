using Jaxx.VideoDb.Data.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jaxx.VideoDb.Data.Context.ContextValidation
{
    public class DiskIdContextValidation
    {
        private VideoDbContext _context;

        public DiskIdContextValidation(VideoDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns true if diskid is already used in database
        /// </summary>
        /// <param name="diskid"></param>
        /// <returns></returns>
        public bool IsDiskIdUsed(string diskid)
        {
            if (!DiskIdValidation.IsValidDiskID(diskid))
                return false;

            bool result;
            var idCount = _context.VideoData.Where(v => v.diskid == diskid).Count();
            if (idCount > 0) result = true;
            else result = false;

            return result;
        }
    }
}
