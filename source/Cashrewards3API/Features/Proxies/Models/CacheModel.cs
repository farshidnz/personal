using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Proxies.Models
{
    public class CacheModel
    {
        [JsonPropertyName("cache_timestamp")]
        public Int64? CacheTimeStamp { get; set; }
        [JsonPropertyName("cache_time")]
        public string CacheTime { get; set; }
        [JsonPropertyName("cache_age")]
        public string CacheAge { get; set; }
    }
}
