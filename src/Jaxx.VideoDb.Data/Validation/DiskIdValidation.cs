using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Jaxx.VideoDb.Data.Validation
{
    public static class DiskIdValidation
    {
        /// <summary>
        /// Checks if the this id matches the pattern.
        /// </summary>
        /// <param name="DiskID"></param>
        /// <returns></returns>
        public static bool IsValidDiskID(string DiskID)
        {
            bool result = false;

            if (DiskID != null)
            {
                Regex regex = new Regex(@"R\d{2}F[1-8]D\d{2}");
                result = regex.IsMatch(DiskID);
            }

            return result;
        }
    }
}
