using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jaxx.WebGallery.Test
{
    public static class TestHelpers
    {
        internal static void PrepareDestinationPath(string path)
        {
            ClearDestinationPath(path);
            Directory.CreateDirectory(path);
        }
        internal static void ClearDestinationPath(string path)
        {
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }
    }
}
