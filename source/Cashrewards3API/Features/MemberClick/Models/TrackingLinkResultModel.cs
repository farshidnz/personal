using System.Collections.Generic;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.MemberClick.Models;

namespace Cashrewards3API.Features.MemberClick
{
    public class TrackingLinkResultModel
    {
        public string FirstName { get; set; }

        public string MerchantName { get; set; }

        public string ClientCommissionString { get; set; }

        public string MerchantImageUrl { get; set; }

        public string MerchantWebsiteUrl { get; set; }

        public MobileAppTrackingTypeEnum MerchantMobileAppTrackingType { get; set; }

        public string TrackingLink { get; set; }

        public int MerchantId { get; set; }

        public string TrackingId { get; set; }

        public int NetworkId { get; set; }

        public int ClickItemId { get; set; }

        public string WwEncryptedClientMemberId { get; set; }

        public string WwEncryptedSiteReferenceId { get; set; }

        public string WwEncryptedTimeStamp { get; set; }
        public List<TrackingLinkResultMerchantTier> Tiers { get; set; }
        public TrackingLinkResultPremiumMerchant Premium { get; set; }
    }
}
