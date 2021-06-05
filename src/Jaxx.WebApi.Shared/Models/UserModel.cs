using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaxx.WebApi.Shared.Models
{
    public class UserModel
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public IEnumerable<string> Groups { get; set; }
    }
}
