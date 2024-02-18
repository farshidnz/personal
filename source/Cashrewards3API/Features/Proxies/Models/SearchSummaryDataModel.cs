using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Proxies.Models
{
    public class SearchSummaryDataModel
    {
        [JsonPropertyName("online")]
        public ItemModel Online { get; set; }

        [JsonPropertyName("offline")]
        public List<ItemModel> Offline { get; set; }
    }
}
