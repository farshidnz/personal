using Cashrewards3API.Common.Dto;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Merchant.Models;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cashrewards3API.Features.Merchant
{
    //TODO : Give it a meaningful name
    public class MerchantByCategorId
    {
        public int MerchantId { get; set; }
    }

    public class MerchantDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string HyphenatedString { get; set; }

        public string LogoUrl { get; set; }

        public string Description { get; set; }

        public decimal Commission { get; set; }

        public bool IsFlatRate { get; set; }

        public string CommissionType { get; set; }

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

        public string BannerImageUrl { get; set; }
        
        public string MerchantBadge { get; set; }

        public bool DesktopEnabled { get; set; }

        public string MobileTrackingNetwork { get; set; }

        public string ClientCommissionString { get; set; }

        public PremiumMerchant Premium { get; set; }

        public bool IsPaused { get; set; }

    }

    public class PopularMerchantDto
    {
        public int MerchantId { get; set; }

        public int NetworkId { get; set; }

        public string MerchantName { get; set; }

        public string logoImageUrl { get; set; }

        public Nullable<bool> IsPopular { get; set; }

        public Nullable<bool> IsMobileAppEnabled { get; set; }


    }

    public partial class MerchantTierView
    {
        public int MerchantId { get; set; }
        public int MerchantTierId { get; set; }
        public string TrackingLink { get; set; }
        public int ClientTierId { get; set; }
        public Nullable<int> ScheduleRateId { get; set; }
        public int TierTypeId { get; set; }
        public int TierCommTypeId { get; set; }
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public int ClientId { get; set; }
        public string TierCssClass { get; set; }
        public string TierName { get; set; }
        public string TierDescription { get; set; }
        public string Identifier { get; set; }
        public Nullable<bool> IsExtra { get; set; }
        public decimal Rate { get; set; }
        public string TierDescriptionLong { get; set; }
        public string TierImageUrl { get; set; }
        public string TierSpecialTerms { get; set; }
        public string TierExclusions { get; set; }
        public string TierReference { get; set; }
        public int Ranking { get; set; }
        public bool IsFeatured { get; set; }
    }

    public class MerchantTierViewWithBadge : MerchantTierView
    {
        public string BadgeCode { get; set; }
    }

    public class PopularStoreConfig
    {
        public PopularStoreConfig()
        {
            MerchantIds = new List<int>();
        }

        public IList<int> MerchantIds { get; set; }
    }

    public class TrendingStoreConfig
    {
        public TrendingStoreConfig()
        {
            MerchantIds = new List<int>();
        }

        public IList<int> MerchantIds { get; set; }
    }

}
