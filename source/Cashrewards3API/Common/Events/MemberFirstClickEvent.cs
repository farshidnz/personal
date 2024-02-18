using Newtonsoft.Json;

namespace Cashrewards3API.Common.Events
{
    public class MemberFirstClickEvent
    {
        [JsonProperty("TargetId")]
        public string TargetId { get; set; }

        [JsonProperty("EmailType")]
        public int EmailType { get; set; }

        [JsonProperty("AdditionalInfo")]
        public string AdditionalInfo { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("TransactionId")]
        public int? TransactionId { get; set; }

        [JsonProperty("MerchantImageUrl")]
        public string MerchantImageUrl { get; set; }
    }
}