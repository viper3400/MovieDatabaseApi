using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.WebApi.Shared.Models
{
    public class RefreshTokenModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string RefreshSessionIdentifier { get; set; }
    }
}
