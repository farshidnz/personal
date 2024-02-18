using Cashrewards3API.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantStore
    {
        public int MerchantId { get; set; }

        public string ReferenceName { get; set; }

        public string Name { get; set; }

        public string Channel { get; set; }

        public string HyphenatedString { get; set; }

        public string LogoUrl { get; set; }

        public string DescriptionShort { get; set; }

        public string DescriptionLong { get; set; }

        public bool IsFlatRate { get; set; }

        public string CommissionType { get; set; } // percent, dollar

        public string Notification { get; set; }

        public string ApprovalTime { get; set; }

        public string TrackingTime { get; set; }

        public string Confirmation { get; set; }

        public string BasicTerms { get; set; }

        public string SpecialTerms { get; set; }

        public string CashbackGuidelines { get; set; }

        public decimal ClientCommission { get; set; }

        public string CardLinkedOfferTerms { get; set; }

        public string ClientCommissionSummary { get; set; }

        public MobileAppTrackingTypeEnum MobileAppTrackingType { get; set; }

        public int TierTypeId { get; set; }

        public List<Tier> Tiers { get; set; }

        public List<Offer> Offers { get; set; }

        public int OfferCount => Offers?.Count ?? 0;

        public bool IsMemberFavourite { get; set; }

        public string MerchantBadgeCode { get; set; }

        public string BackgroundImageUrl { get; set; }

        public string BannerImageUrl { get; set; }

        public bool IsMobileEnabled { get; set; }

        public bool IsDesktopEnabled { get; set; }

        public bool IsMobileAppEnabled { get; set; }

        public MobileTrackingNetworkEnum MobileTrackingNetwork { get; set; }

        public MerchantStorePremium Premium { get; set; }

        public bool IsGiftCard { get; set; }

        public bool IsPaused { get; set; }

        public class Tier
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string ClientCommissionString { get; set; }

            public decimal ClientCommission { get; set; }

            public string CommissionType { get; set; }

            public string EndDateTime { get; set; }

            public string Terms { get; set; }

            public string Exclusions { get; set; }

            public List<MerchantTierLink> TierLinks { get; set; }

            public string TierReference { get; set; }

            public string TierSpecialTerms { get; set; }

            public bool IsFeatured { get; set; }

            public int Ranking { get; set; }

            public string DescriptionLong { get; set; }

            public string TierImageUrl { get; set; }

            public string BadgeCode { get; set; }

            public PremiumTier Premium { get; set; }

            public string TrackingLink { get; set; }
        }

        public class Offer
        {
            public string Title { get; set; }

            public string CouponCode { get; set; }

            public string EndDateTime { get; set; }

            public string Description { get; set; }

            public string HyphenatedString { get; set; }

            public bool IsFeatured { get; set; }

            public string Terms { get; set; }

            public int Id { get; set; }

            public string ClientCommissionString { get; set; }

            public string MerchantLogoUrl { get; set; }

            public int MerchantId { get; set; }

            public bool IsCashbackIncreased { get; set; }

            public string OfferBackgroundImageUrl { get; set; }

            public string OfferBadgeCode { get; set; }

            public bool IsPremium { get; set; }

            public bool IsPremiumFeature { get; set; }

        }

    }

    public class MerchantStorePremium
    {
        public decimal ClientCommission { get; set; }
        public string ClientCommissionSummary { get; set; }
        public bool IsFlatRate { get; set; }
        public string Notification { get; set; }
        public string Confirmation { get; set; }
        public string TrackingTime { get; set; }
        public string ApprovalTime { get; set; }
    }

    public class PremiumTier
    {
        public string ClientCommissionString { get; set; }
        public decimal ClientCommission { get; set; }
        public string CommissionType { get; set; }
    }
}

