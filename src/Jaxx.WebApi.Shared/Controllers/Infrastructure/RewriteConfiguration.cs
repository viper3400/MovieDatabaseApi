using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.WebApi.Shared.Infrastructure
{
    public class RewriteConfiguration : IRewriteConfiguration
    {
        public string Protcol { get; set; }
        public string RewriteUrl { get; set; }
    }
}
