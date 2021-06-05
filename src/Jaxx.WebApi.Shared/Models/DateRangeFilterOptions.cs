using System;
using System.Collections.Generic;
using System.Text;

namespace Jaxx.WebApi.Shared.Models
{
    public class DateRangeFilterOptions
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
