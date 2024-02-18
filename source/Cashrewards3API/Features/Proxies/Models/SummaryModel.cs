using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Proxies.Models
{
    public class SummaryModel
    {
        [JsonPropertyName("totalCount")]
        public int? TotalCount { get; set; }
        [JsonPropertyName("count")]
        public int? Count { get; set; }
        [JsonPropertyName("data")]
        public List<SearchSummaryDataModel> Data {get; set; }
    }
    
}
