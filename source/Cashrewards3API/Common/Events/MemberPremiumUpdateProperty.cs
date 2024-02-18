using Newtonsoft.Json;

namespace Cashrewards3API.Common.Events
{
    public class MemberPremiumUpdateProperty
    {
        [JsonProperty("PremiumStatus")]
        public int PremiumStatus { get; set; }

        [JsonProperty("ExternalMemberId")]
        public string ExternalMemberId { get; set; }
    }
}