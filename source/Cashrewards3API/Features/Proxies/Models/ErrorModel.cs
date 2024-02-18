using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cashrewards3API.Features.Proxies.Models
{
    public class ErrorModel
    {
        [JsonProperty("errors")]
        public List<Error> Errors { get; set; }
    }

    public class Error
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
