using Cashrewards3API.Common.Dto;
using Cashrewards3API.Enum;

namespace Cashrewards3API.Features.MemberClick
{
    public class TrackingLinkInfoModel
    {
        public int ClientId { get; set; }

        public int? PremiumClientId { get; set; }

        public int? CampaignId { get; set; }

        public string HyphenatedString { get; set; }

        public string HyphenatedStringWithType { get; set; }


        public string IpAddress { get; set; }

        public string UserAgent { get; set; }

        public bool IsMobileApp { get; set; }

        public string MemberClickItemTypeString { get; set; }

        public MemberClickItemTypeEnum MemberClickType { get; set; }

        public MerchantModel Merchant { get; set; }

        public int ClickItemId { get; set; }

        public string ClickItemImageUrl { get; set; }

        public string TrackingLinkTemplate { get; set; }

        public MemberContextModel Member { get; set; }

        public NetworkModel Network { get; set; }

        public MerchantModel PremiumMerchant { get; set; }
        public bool IncludeTiers { get; set; }
    }
}
