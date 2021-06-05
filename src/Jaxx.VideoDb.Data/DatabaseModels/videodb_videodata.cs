using Jaxx.VideoDb.Data.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.VideoDb.Data.DatabaseModels
{
    public partial class videodb_videodata
    {
        public int id { get; set; }
        public string md5 { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public string language { get; set; }
        public string diskid { get; set; }
        public string comment { get; set; }
        public string disklabel { get; set; }
        public string imdbID { get; set; }
        public int year { get; set; }
        public string imgurl { get; set; }
        public string director { get; set; }
        public string actors { get; set; }
        public int? runtime { get; set; }
        public string country { get; set; }
        public string plot { get; set; }
        public string rating { get; set; }
        public string filename { get; set; }
        public Int64? filesize { get; set; }
        public DateTime? filedate { get; set; }
        public string audio_codec { get; set; }
        public string video_codec { get; set; }
        public int? video_width { get; set; }
        public int? video_height { get; set; }
        public bool istv { get; set; }
        public DateTime lastupdate { get; set; }
        public int mediatype { get; set; }
        public string custom1 { get; set; }
        public string custom2 { get; set; }
        public string custom3 { get; set; }
        public string custom4 { get; set; }
        public DateTime? created { get; set; }
        public int owner_id { get; set; }
    }
}
