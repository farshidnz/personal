using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Proxies.Models
{
    public class AddCardRequestModel
    {
        public string CardNumber { get; set; }
        public string MemberId { get; set; }
    }
}
