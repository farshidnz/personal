using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Formatting.Display;

namespace Cashrewards3API.Features.Member.Request.UpdateCognitoMember
{
    public class UpdateFacebookUsernameRequest
    {
        public string FacebookUsername { get; set; }
    }
}
