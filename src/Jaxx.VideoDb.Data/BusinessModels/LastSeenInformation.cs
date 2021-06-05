using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.VideoDb.Data.BusinessModels
{
    public class LastSeenInformation
    {
        public DateTime LastSeenDate { get; set; }
        public int SeenCount { get; set; }
        public int DaysSinceLastView { get; set; }
        public string LastSeenSentence { get; set; }
        public string ReadableTimeSinceLastViewHtml { get; set; }
        public List<DateTime> AllSeenDates { get; set; }
    }
}
