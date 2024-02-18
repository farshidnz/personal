using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Proxies.Models
{
    public class ItemModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("logoUrl")]
        public string LogoUrl { get; set; }
        [JsonPropertyName("commission")]
        public string Commission { get; set; }
        [JsonPropertyName("isFlatRate")]
        public string IsFlatRate { get; set; }
        [JsonPropertyName("commissiontype")]
        public string CommissionType { get; set; }
        [JsonPropertyName("offercount")]
        public string OfferCount { get; set; }
        [JsonPropertyName("rewardType")]
        public string RewardType { get; set; }
        [JsonPropertyName("mobileTrackingType")]
        public string MobileTrackingType { get; set; }
    }
}
