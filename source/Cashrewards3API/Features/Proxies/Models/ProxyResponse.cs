using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Proxies.Models
{
    public class ProxyResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ResponseText { get; set; }
    }
}
