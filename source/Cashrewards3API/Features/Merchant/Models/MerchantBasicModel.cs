using Cashrewards3API.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantBasicModel : WithBannerUrl, IHaveIsPaused
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string HyphenatedString { get; set; }

        public string LogoUrl { get; set; }

        public string Description { get; set; }

        public decimal Commission { get; set; }

        public bool IsFlatRate { get; set; }

        public string CommissionType { get; set; }

        public string CommissionString { get; set; }

        public int? TrackingTimeMin { get; set; }

        public int? TrackingTimeMax { get; set; }

        public int? ApprovalTime { get; set; }

        public string SpecialTerms { get; set; }

        public string CashbackGuidelines { get; set; }

        public decimal OfferCount { get; set; }

        public string RewardType { get; set; }

        public string MobileTrackingType { get; set; }

        public bool IsCustomTracking { get; set; }

        public string BackgroundImageUrl { get; set; }

        public string MerchantBadge { get; set; }

        public bool DesktopEnabled { get; set; }

        public string MobileTrackingNetwork { get; set; }

        public MerchantBasicPremiumModel Premium { get; set; }

        public bool IsPaused { get; set; }
    }

    public class MerchantBasicPremiumModel
    {
        public decimal Commission { get; set; }
        public bool IsFlatRate { get; set; }
        public string CommissionString { get; set; }
        public int? TrackingTimeMin { get; set; }
        public int? TrackingTimeMax { get; set; }
        public int? ApprovalTime { get; set; }
    }
}
