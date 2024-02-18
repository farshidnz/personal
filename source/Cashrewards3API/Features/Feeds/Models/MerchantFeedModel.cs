using System.Collections.Generic;

namespace Cashrewards3API.Features.Feeds.Models
{
    public class MerchantFeedModel
    {
        public int MerchantId { get; set; }
        public string MerchantName { get; set; }
        public string MerchantDescription { get; set; }
        public string MerchantTrackingUrl { get; set; }
        public string MerchantWebsite { get; set; }
        public IEnumerable<MerchantFeedTierModel> MerchantTier { get; set; }
    }

    public class MerchantFeedTierModel
    {
        public string TierDescription { get; set; }
        public decimal TierCashback { get; set; }
        public string TierCashbackType { get; set; }
        public MerchantFeedTierPremiumModel Premium { get; set; }
    }

    public class MerchantFeedTierPremiumModel
    {
        public decimal TierCashback { get; set; }
        public string TierCashbackType { get; set; }
    }
}
