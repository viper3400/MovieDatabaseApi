using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieDatabaseCLI
{
    public class SyncResultModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string FilePath { get; set; }

        public bool FileExistsOnStorage { get; set; }

        public int Deleted { get; set; } =-1;

    }
}
