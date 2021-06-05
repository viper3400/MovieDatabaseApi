using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.Data.BusinessModels
{
    public class MovieDataServiceOptions
    {
        public int DeletedUserId { get; set; }
        public string LocalCoverImagePath { get; set; }
        public string HttpCoverImagePath { get; set; }
        public string LocalBackgroundImagePath { get; set; }

        public IEnumerable<int> MediaTypesFilter { get; set; }
    }
}
