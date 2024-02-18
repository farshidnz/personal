using System;
using Cashrewards3API.Common.Model;
using Cashrewards3API.Features.Merchant.Models;
using Dapper.Contrib.Extensions;

namespace Cashrewards3API.Features.Offers
{
    [Table("MaterialisedOfferView")]
    public class OfferViewModel: IHaveIsMerchantPaused
    {
        [Key]
        public int OfferId { get; set; }
        public int ClientId { get; set; }
        public int MerchantId { get; set; }
        public bool IsFeatured { get; set; }
        public string CouponCode { get; set; }
        public string OfferTitle { get; set; }
        public string AlteredOfferTitle { get; set; }
        public string MerchantTrackingLink { get; set; }
        public string OfferDescription { get; set; }
        public string TrackingLink { get; set; }
        public string HyphenatedString { get; set; }
        public System.DateTime DateEnd { get; set; }
        public string CaptionCssClass { get; set; }
        public string MerchantName { get; set; }
        public string RegularImageUrl { get; set; }
        public string BasicTerms { get; set; }
        public string SmallImageUrl { get; set; }
        public string MediumImageUrl { get; set; }
        public Nullable<int> OfferCount { get; set; }
        public int TierCommTypeId { get; set; }
        public int TierTypeId { get; set; }
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal ClientCommission => Math.Round(Commission * (ClientComm / 100) * (MemberComm / 100), 2);
        public decimal MemberComm { get; set; }
        public string TierCssClass { get; set; }
        public string ExtentedTerms { get; set; }
        public string RewardName { get; set; }
        public string MerchantShortDescription { get; set; }
        public string MerchantHyphenatedString { get; set; }
        public int NetworkId { get; set; }
        public string TrackingTime { get; set; }
        public string ApprovalTime { get; set; }
        public string OfferTerms { get; set; }
        public int ClientProgramTypeId { get; set; }
        public Nullable<bool> IsFlatRate { get; set; }
        public decimal Rate { get; set; }
        public string NotificationMsg { get; set; }
        public string ConfirmationMsg { get; set; }
        public Nullable<bool> IsToolbarEnabled { get; set; }
        public Nullable<int> TierCount { get; set; }
        public int Ranking { get; set; }
        public Nullable<System.Guid> RandomNumber { get; set; }
        public string MerchantMetaDescription { get; set; }
        public string OfferBackgroundImageUrl { get; set; }
        public string OfferBadgeCode { get; set; }
        public bool IsCashbackIncreased { get; set; }
        public string MerchantBadgeCode { get; set; }
        public string OfferPastRate { get; set; }
        public bool IsCategoryFeatured { get; set; }
        public string ClientCommissionString => MerchantTierSummaryBase.GetCommissionString(ClientProgramTypeId, TierCommTypeId, ClientCommission, Rate, IsFlatRate, TierTypeId, RewardName);

        public bool IsPremiumFeature { get; set; }

        public bool? MerchantIsPremiumDisabled { get; set; }

        public bool IsMerchantPaused { get; set; }
    }

    public class OfferDto: IHaveIsMerchantPaused
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string CouponCode { get; set; }

        public DateTime EndDateTime { get; set; }

        public string Description { get; set; }

        public string HyphenatedString { get; set; }

        public bool IsFeatured { get; set; }

        public string Terms { get; set; }

        public int MerchantId { get; set; }

        public string MerchantLogoUrl { get; set; }

        public string OfferBackgroundImageUrl { get; set; }

        public string OfferBadge { get; set; }

        public bool IsCashbackIncreased { get; set; }

        public bool IsPremiumFeature { get; set; }

        public string WasRate { get; set; }

        public MerchantBasicModel Merchant { get; set; }

        public Premium Premium { get; set; }

        public string ClientCommissionString { get; set; }

        public string RegularImageUrl { get; set; }

        public string OfferPastRate { get; set; }

        public string MerchantHyphenatedString { get; set; }
        public bool IsMerchantPaused { get; set; }
    }

    public class MerchantBasicModel
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

        public int? OfferCount { get; set; }

        public string RewardType { get; set; }

        public string MobileTrackingType { get; set; }

        public bool IsCustomTracking { get; set; }

        public string BackgroundImageUrl { get; set; }

        public string BannerImageUrl { get; set; }

        public string MerchantBadge { get; set; }

        public bool DesktopEnabled { get; set; }

        public string MobileTrackingNetwork { get; set; }
        public bool IsPaused { get; set; }
    }

    public class Premium
    {
        public decimal Commission { get; set; }
        public bool IsFlatRate { get; set; }
        public string ClientCommissionString { get; set; }
    }

    [Table("MaterialisedMerchantFullView")]
    public class MobileDisabledMerchnt
    {
        [Key]
        public int MerchantId { get; set; }
        public int ClientId { get; set; }
        public bool? IsMobileAppEnabled { get; set; }
    }

    [Table("MaterialisedMerchantFullView")]
    public class OfferMerchantModel
    {
        public int MerchantId { get; set; }

        public int TierCommTypeId { get; set; }

        public int TierTypeId { get; set; }

        public decimal Commission { get; set; }

        public decimal ClientComm { get; set; }

        public decimal MemberComm { get; set; }

        public decimal Rate { get; set; }

        public int ClientProgramTypeId { get; set; }

        public Nullable<bool> IsFlatRate { get; set; }

        public string RewardName { get; set; }

        public decimal ClientCommission => Math.Round(Commission * (ClientComm / 100) * (MemberComm / 100), 2);

        public string ClientCommissionString => MerchantTierSummaryBase.GetCommissionString(ClientProgramTypeId, TierCommTypeId, ClientCommission, Rate, IsFlatRate, TierTypeId, RewardName);
    }
}
