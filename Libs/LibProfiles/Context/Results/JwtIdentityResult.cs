using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibProfiles.Context.Results
{
    public class JwtIdentityResult
    {
        public string access_token { get; set; }
        public string username { get; set; }
    }
}
