using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Enum;
using Cashrewards3API.Extensions;
using Cashrewards3API.Features.Feeds.Models;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;



namespace Cashrewards3API.Mapper
{
    public class MerchantProfile : Profile
    {
        public MerchantProfile()
        {
            CreateMap<MerchantViewModel, PremiumMerchant>()
                .ForMember(dest => dest.Commission, opts => opts.MapFrom(src => Math.Round((src.Commission * src.ClientComm * src.MemberComm) / 10000, 2)));

            CreateMap<MerchantFullView, MerchantStore>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.MerchantName))
                .ForMember(dest => dest.LogoUrl, opts => opts.MapFrom(src => src.RegularImageUrl))
                .ForMember(dest => dest.CommissionType, opts => opts.MapFrom(src => CommissionTypeDict[src.TierCommTypeId]))
                .ForMember(dest => dest.ClientCommissionSummary, opts => opts.MapFrom(src => src.ClientCommissionString))
                .ForMember(dest => dest.MobileAppTrackingType, opts => opts.MapFrom(src =>
                    src.MobileAppTrackingType.HasValue && System.Enum.IsDefined(typeof(MobileAppTrackingTypeEnum), src.MobileAppTrackingType)
                    ? (MobileAppTrackingTypeEnum)src.MobileAppTrackingType
                    : MobileAppTrackingTypeEnum.InAppBrowser))
                .ForMember(dest => dest.Notification, opts => opts.MapFrom(src => src.NotificationMsg))
                .ForMember(dest => dest.SpecialTerms, opts => opts.MapFrom(src => src.ExtentedTerms))
                .ForMember(dest => dest.CashbackGuidelines, opts => opts.MapFrom(src => src.CashbackGuideLine))
                .ForMember(dest => dest.Channel, opts => opts.MapFrom(src => GetChannelName(src)))
                .ForMember(dest => dest.Confirmation, opts => opts.MapFrom(src => src.ConfirmationMsg))
                .ForMember(dest => dest.IsMobileEnabled, opts => opts.MapFrom(src => src.MobileEnabled ?? false))
                .ForMember(dest => dest.IsDesktopEnabled, opts => opts.MapFrom(src => src.DesktopEnabled))
                .ForMember(dest => dest.IsMobileAppEnabled, opts => opts.MapFrom(src => src.IsMobileAppEnabled ?? false))
                .ForMember(dest => dest.MobileTrackingNetwork, opts => opts.MapFrom(src =>
                    src.MobileTrackingNetwork.HasValue && System.Enum.IsDefined(typeof(MobileTrackingNetworkEnum), src.MobileTrackingNetwork)
                    ? (MobileTrackingNetworkEnum)src.MobileTrackingNetwork
                    : MobileTrackingNetworkEnum.Button))
                .ForMember(dest => dest.Premium, opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.Premium]));

            CreateMap<MerchantFullView, OfflineMerchantStore>()
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.MerchantName))
                .ForMember(dest => dest.LogoUrl, opts => opts.MapFrom(src => src.RegularImageUrl))
                .ForMember(dest => dest.CommissionType, opts => opts.MapFrom(src => CommissionTypeDict[src.TierCommTypeId]))
                .ForMember(dest => dest.ClientCommissionSummary, opts => opts.MapFrom(src => src.ClientCommissionString))
                .ForMember(dest => dest.MobileAppTrackingType, opts => opts.MapFrom(src =>
                    src.MobileAppTrackingType.HasValue && System.Enum.IsDefined(typeof(MobileAppTrackingTypeEnum), src.MobileAppTrackingType)
                    ? (MobileAppTrackingTypeEnum)src.MobileAppTrackingType
                    : MobileAppTrackingTypeEnum.InAppBrowser))
                .ForMember(dest => dest.Notification, opts => opts.MapFrom(src => src.NotificationMsg))
                .ForMember(dest => dest.SpecialTerms, opts => opts.MapFrom(src => src.ExtentedTerms))
                .ForMember(dest => dest.CashbackGuidelines, opts => opts.MapFrom(src => src.CashbackGuideLine))
                .ForMember(dest => dest.Channel, opts => opts.MapFrom(src => GetChannelName(src)))
                .ForMember(dest => dest.Confirmation, opts => opts.MapFrom(src => src.ConfirmationMsg))
                .ForMember(dest => dest.IsMobileEnabled, opts => opts.MapFrom(src => src.MobileEnabled ?? false))
                .ForMember(dest => dest.IsDesktopEnabled, opts => opts.MapFrom(src => src.DesktopEnabled))
                .ForMember(dest => dest.IsMobileAppEnabled, opts => opts.MapFrom(src => src.IsMobileAppEnabled ?? false))
                .ForMember(dest => dest.MobileTrackingNetwork, opts => opts.MapFrom(src =>
                    src.MobileTrackingNetwork.HasValue && System.Enum.IsDefined(typeof(MobileTrackingNetworkEnum), src.MobileTrackingNetwork)
                    ? (MobileTrackingNetworkEnum)src.MobileTrackingNetwork
                    : MobileTrackingNetworkEnum.Button))
                .ForMember(dest => dest.Premium, opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.Premium]));



            CreateMap<MerchantFullView, MerchantStorePremium>()
                .ForMember(dest => dest.ClientCommissionSummary, opts => opts.MapFrom(src => src.ClientCommissionString));

            CreateMap<MerchantStoresBundle, MerchantBundleBasicModel>()
                .ForMember(dest => dest.Online, opts => opts.MapFrom(src => src.OnlineStore))
                .ForMember(dest => dest.Offline, opts => opts.MapFrom(src => src.OfflineStores));

            CreateMap<MerchantStore, MerchantBasicModel>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.MerchantId))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.DescriptionLong))
                .ForMember(dest => dest.Commission, opts => opts.MapFrom(src => src.ClientCommission))
                .ForMember(dest => dest.CommissionString, opts => opts.MapFrom(src => src.ClientCommissionSummary))
                .ForMember(dest => dest.TrackingTimeMin, opts => opts.MapFrom(src => GetTrackingTimeMin(src.TrackingTime)))
                .ForMember(dest => dest.TrackingTimeMax, opts => opts.MapFrom(src => GetTrackingTimeMax(src.TrackingTime)))
                .ForMember(dest => dest.ApprovalTime, opts => opts.MapFrom(src => GetApprovalTime(src.ApprovalTime)))
                .ForMember(dest => dest.RewardType, opts => opts.MapFrom(src => GetRewardsType(src.TierTypeId)))
                .ForMember(dest => dest.MobileTrackingType, opts => opts.MapFrom(src => src.MobileAppTrackingType.GetEnumDescription()))
                .ForMember(dest => dest.IsCustomTracking, opts => opts.MapFrom(
                    (src, _, __, context) => IsCustomTracking(src.MerchantId, (string)context.Items[Constants.Mapper.CustomTrackingMerchantList])))
                .ForMember(dest => dest.MerchantBadge, opts => opts.MapFrom(src => src.MerchantBadgeCode))
                .ForMember(dest => dest.DesktopEnabled, opts => opts.MapFrom(src => src.IsDesktopEnabled))
                .ForMember(dest => dest.MobileTrackingNetwork, opts => opts.MapFrom(
                    src => src.MobileAppTrackingType == MobileAppTrackingTypeEnum.AppToApp ? src.MobileTrackingNetwork.GetEnumDescription() : string.Empty));

            CreateMap<MerchantStorePremium, MerchantBasicPremiumModel>()
                .ForMember(dest => dest.Commission, opts => opts.MapFrom(src => src.ClientCommission))
                .ForMember(dest => dest.CommissionString, opts => opts.MapFrom(src => src.ClientCommissionSummary))
                .ForMember(dest => dest.TrackingTimeMin, opts => opts.MapFrom(src => GetTrackingTimeMin(src.TrackingTime)))
                .ForMember(dest => dest.TrackingTimeMax, opts => opts.MapFrom(src => GetTrackingTimeMax(src.TrackingTime)))
                .ForMember(dest => dest.ApprovalTime, opts => opts.MapFrom(src => GetApprovalTime(src.ApprovalTime)));

            CreateMap<OfflineMerchantStore, CardLinkedBasicMerchantModel>()
                .ForMember(dest => dest.VisaMeta, opts => opts.MapFrom(src => GetVisaMetaData(src)))
                .ForMember(dest => dest.MinimumSpend, opts => opts.MapFrom(src => ExtractMinimumSpend(src.Tiers)))
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.MerchantId))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.DescriptionLong))
                .ForMember(dest => dest.Commission, opts => opts.MapFrom(src => src.ClientCommission))
                .ForMember(dest => dest.CommissionString, opts => opts.MapFrom(src => src.ClientCommissionSummary))
                .ForMember(dest => dest.TrackingTimeMin, opts => opts.MapFrom(src => GetTrackingTimeMin(src.TrackingTime)))
                .ForMember(dest => dest.TrackingTimeMax, opts => opts.MapFrom(src => GetTrackingTimeMax(src.TrackingTime)))
                .ForMember(dest => dest.ApprovalTime, opts => opts.MapFrom(src => GetApprovalTime(src.ApprovalTime)))
                .ForMember(dest => dest.RewardType, opts => opts.MapFrom(src => GetRewardsType(src.TierTypeId)))
                .ForMember(dest => dest.MobileTrackingType, opts => opts.MapFrom(src => src.MobileAppTrackingType.GetEnumDescription()))
                .ForMember(dest => dest.IsCustomTracking, opts => opts.MapFrom(
                    (src, _, __, context) => IsCustomTracking(src.MerchantId, (string)context.Items[Constants.Mapper.CustomTrackingMerchantList])))
                .ForMember(dest => dest.MerchantBadge, opts => opts.MapFrom(src => src.MerchantBadgeCode))
                .ForMember(dest => dest.DesktopEnabled, opts => opts.MapFrom(src => src.IsDesktopEnabled))
                .ForMember(dest => dest.MobileTrackingNetwork, opts => opts.MapFrom(
                    src => src.MobileAppTrackingType == MobileAppTrackingTypeEnum.AppToApp ? src.MobileTrackingNetwork.GetEnumDescription() : string.Empty));

            CreateMap<MerchantBasicPremiumModel, MerchantStorePremium>()
               .ForMember(dest => dest.ClientCommission, opts => opts.MapFrom(src => src.Commission))
               .ForMember(dest => dest.IsFlatRate, opts => opts.MapFrom(src => src.IsFlatRate));

            CreateMap<MerchantFeedDataModel, MerchantFeedModel>()
                .ForMember(dest => dest.MerchantDescription, opts => opts.MapFrom(src => src.DescriptionShort))
                .ForMember(dest => dest.MerchantWebsite, opts => opts.MapFrom(src => src.WebsiteUrl))
                .ForMember(dest => dest.MerchantTrackingUrl, opts => opts.MapFrom(src => $"http://cashrewards.com.au/{src.HyphenatedString}/"))
                .ForMember(dest => dest.MerchantTier, opts => opts.MapFrom((_, _, _, context) => context.Items[Constants.Mapper.Tiers]));

            CreateMap<MerchantFeedTierDataModel, MerchantFeedTierModel>()
                .ForMember(dest => dest.TierCashback, opts => opts.MapFrom(src => GetMerchantFeedCashback(src)))
                .ForMember(dest => dest.TierCashbackType, opts => opts.MapFrom(src => src.TierCommTypeId == (int)TierCommTypeEnum.Dollar ? "Dollar Value" : "Percentage Value"));

            CreateMap<MerchantFeedTierDataModel, MerchantFeedTierPremiumModel>()
                .ForMember(dest => dest.TierCashback, opts => opts.MapFrom(src => GetMerchantFeedCashback(src)))
                .ForMember(dest => dest.TierCashbackType, opts => opts.MapFrom(src => src.TierCommTypeId == (int)TierCommTypeEnum.Dollar ? "Dollar Value" : "Percentage Value"));
        }

        private static decimal GetMerchantFeedCashback(MerchantFeedTierDataModel src)
        {
            return (src.Commission * (src.ClientComm / 100) * (src.MemberComm / 100)).RoundToTwoDecimalPlaces();
        }

        public static readonly Dictionary<int, string> CommissionTypeDict = new Dictionary<int, string>()
        {
            [100] = "dollar",
            [101] = "percent"
        };

        private static readonly Dictionary<int, string> NetworkChannelDict = new Dictionary<int, string>()
        {
            [1000053] = "In-Store Visa",
            [1000059] = "Online Visa"
        };

        public static string GetChannelName(MerchantFullView merchant)
        {
            if (NetworkChannelDict.ContainsKey(merchant.NetworkId))
            {
                return "visa";
            }
            return string.Empty;
        }

        //TODO: TrackingTime and ApprovalTime fields are string in the database, format: "Up to 7 Days", "1 to 7 Days" or "Up to 90 days from rental completion"
        //it is impossible to control the exact format of these values as it can be entered in different ways, including "Immediate" etc. 
        //we need to consider to modify DB and store only digital value and allow frontend to construct the text to show in 'scheduling' area.
        // 
        // Source of the expected values to be passed into the Get*Time functions
        // SELECT DISTINCT [ApprovalTime] FROM [ShopGo].[dbo].[Merchant]
        // SELECT DISTINCT [TrackingTime] FROM [ShopGo].[dbo].[Merchant]

        /// <summary>
        /// Returns the minimum tracking time if present for a merchant.
        /// Attempts to match case where "*{minimum} to {maximum} day*"
        /// is present in the string for the minimum value.
        /// Eg; "1 to 7 days5" resolves to 1
        /// </summary>
        /// <param name="trackingTime">Entry in [ShopGo].[dbo].[Merchant][TrackingTime]</param>
        /// <returns></returns>
        public static int? GetTrackingTimeMin(string trackingTime)
        {
            Regex regex = new Regex(@"(?i)[1-9]\d*(?=(\s*to \d+ day))");
            return trackingTime.ToIntOrNull(regex);
        }

        /// <summary>
        /// Returns the maximum tracking time if present for a merchant.
        /// Attempts to match case where "*{days} day*"
        /// is present in the string for the maximum value
        /// Eg; "1 to 7 days5" resolves to 7
        /// </summary>
        /// <param name="trackingTime">Entry in [ShopGo].[dbo].[Merchant][TrackingTime]</param>
        /// <returns></returns>
        public static int? GetTrackingTimeMax(string trackingTime)
        {
            Regex regex = new Regex(@"(?i)[1-9]\d*(?= day)");
            return trackingTime.ToIntOrNull(regex);
        }

        /// <summary>
        /// Returns the approval time if present for a merchant.
        /// Attempts to match multiple cases due to the inconsistant
        /// data in the table column.
        /// "maximum {days} day*", "up to {days} day*"
        /// "*up to {days}", "*up to {days}day*"
        /// "{days} day*"
        /// Eg; "maximum 7 day9" resolves to 7
        /// </summary>
        /// <param name="strApprovalTime">Entry in [ShopGo].[dbo].[Merchant][ApprovalTime]</param>
        /// <returns></returns>
        public static int? GetApprovalTime(string strApprovalTime)
        {
            Regex regex = new Regex(@"(?i)(?<=^up to |^|^maximum |0)[1-9]\d*(?= *?day|$)");
            return strApprovalTime.ToIntOrNull(regex);
        }

        public static bool IsCustomTracking(int merchantId, string customTrackingMerchantList)
        {
            var merchantList = customTrackingMerchantList
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => System.Convert.ToInt32(n))
                .ToArray();

            return merchantList.Contains(merchantId);
        }

        public static string GetRewardsType(int tierTypeId)
        {
            return GetRewardTypeString(tierTypeId);
        }

        private static string GetRewardTypeString(int TierTypeId)
        {
            if (TierTypeId == 121 || TierTypeId == 117)
            {
                return Constants.RewardTypes.Savings;
            }
            return Constants.RewardTypes.Cashback;
        }

        public static string GetMerchantCommissionTypeString(int tierCommTypeId)
        {
            return GetMerchantCommissionTypeStringCore(tierCommTypeId);
        }

        private static string GetMerchantCommissionTypeStringCore(int tierCommTypeId)
        {
            switch (tierCommTypeId)
            {
                case 100:
                    return Constants.MerchantCommissionType.Dollar;

                case 101:
                    return Constants.MerchantCommissionType.Percent;

                default:
                    return string.Empty;
            }
        }



        public static VisaMetaData GetVisaMetaData(MerchantStore merchantStore)
        {
            if (merchantStore.Channel != "visa")
                return null;
            var visaMeta = new VisaMetaData { MerchantGroupName = merchantStore.ReferenceName, OfferId = merchantStore.Tiers?.FirstOrDefault()?.TierReference };

            DateTime offerExpiry;
            var tierExpiry = merchantStore.Tiers?.FirstOrDefault()?.EndDateTime;
            if (!string.IsNullOrEmpty(tierExpiry))
            {
                if (DateTime.TryParse(tierExpiry, out offerExpiry))
                {
                    visaMeta.OfferExpiry = offerExpiry;
                }
            }

            return visaMeta;
        }



        private static readonly Regex MinimumSpendRegex = new Regex(@"\$(\d*\.?\d*)");



        private decimal? ExtractMinimumSpend(List<MerchantStore.Tier> tiers)
        {
            var firstTierTerm = tiers?.FirstOrDefault()?.TierSpecialTerms;
            if (!string.IsNullOrEmpty(firstTierTerm))
            {
                var matches = MinimumSpendRegex.Match(firstTierTerm);
                if (matches.Success && matches.Groups.Count > 1)
                {
                    var spendString = matches.Groups[1].Value;
                    if (!string.IsNullOrEmpty(spendString))
                    {
                        decimal result;
                        var success = Decimal.TryParse(spendString, out result);
                        if (success)
                        {
                            return result;
                        }
                    }
                }
            }

            return null;
        }

    }
}