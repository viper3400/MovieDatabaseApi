using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.WebApi.Shared.Models
{
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Group { get; set; }
        public string ApiMasterkey { get; set; }
    }
}
