using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Helpers;
using Cashrewards3API.Mapper;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using System.Text.RegularExpressions;
using static Cashrewards3API.Common.Constants;
using Cashrewards3API.Features.Offers;

namespace Cashrewards3API.Features.Merchant
{
    public interface IMerchantMappingService
    {
        MerchantStore ConvertToMerchantStore(MerchantFullView merchantFullView, bool force2DecimalCommissionString = false);
        OfflineMerchantStore ConvertToOfflineMerchantStore(MerchantFullView merchantFullView, bool force2DecimalCommissionString = false);
        MerchantStorePremium ConvertToPremiumMerchant(MerchantFullView merchantFullView, bool force2DecimalCommissionString = false);
        IEnumerable<MerchantDto> ConvertToMerchantDto(IEnumerable<MerchantViewModel> merchants);
        MerchantStore.Tier ConvertToMerchantStoreTier(MerchantTier merchantTier, bool force2DecimalCommissionString = false);
        MerchantStore.Offer ConvertToOffer(OfferViewModel offer, int? premiumClientId = null, bool force2DecimalCommissionString = false);
        MerchantTier ConvertToMerchantTierDto(MerchantTierView merchantTierView);
        MerchantTier ConvertToMerchantTierDtoWithBadgeCode(MerchantTierViewWithBadge merchantTierView);
        MerchantTierLink ConvertToMerchantTierLink(MerchantTierLinkModel merchantTierLink);
        OfflineMerchantStore.Store ConvertToOfflineStore(StoreModel store);
        MerchantBundleDetailResultModel ConvertToMerchantBundleDetailResultModel(MerchantStoresBundle merchantStoresBundles);
        IEnumerable<AllStoresMerchantModel> ConvertToAllStoresMerchantModel(IEnumerable<MerchantStoresBundle> merchantStoresBundles, IEnumerable<MerchantFullView> premiumMerchants);
        PremiumTier ConvertToPremiumTier(MerchantTier merchantTier, bool force2DecimalCommissionString = false);
    }

    public class MerchantMappingService : IMerchantMappingService
    {
        private readonly IConfiguration Configuration;
        private readonly IMapper _mapper;
        private readonly string CustomTrackingMerchantList;

        private static readonly Dictionary<int, string> StateIdDict = new Dictionary<int, string>()
        {
            [1] = "NSW",
            [2] = "QLD",
            [3] = "SA",
            [4] = "TAS",
            [5] = "VIC",
            [6] = "WA",
            [7] = "ACT",
            [8] = "NT"
        };

        public MerchantMappingService(IConfiguration configuration, IMapper mapper)
        {
            Configuration = configuration;
            CustomTrackingMerchantList = Configuration["Config:CustomTrackingMerchantList"];
            _mapper = mapper;

        }

        public MerchantStore ConvertToMerchantStore(MerchantFullView merchantFullView, bool force2DecimalCommissionString = false)
        {
            if (merchantFullView == null)
                return null;

            var src = merchantFullView;

            return new MerchantStore
            {
                MerchantId = src.MerchantId,
                Name = src.MerchantName,
                HyphenatedString = src.HyphenatedString,
                LogoUrl = src.RegularImageUrl,
                DescriptionShort = src.DescriptionShort,
                DescriptionLong = src.DescriptionLong,
                IsFlatRate = src.IsFlatRate ?? false,
                CommissionType = MerchantProfile.CommissionTypeDict[src.TierCommTypeId],
                TierTypeId = src.TierTypeId,
                MobileAppTrackingType = src.MobileAppTrackingType.HasValue && System.Enum.IsDefined(typeof(MobileAppTrackingTypeEnum), src.MobileAppTrackingType) ?
                                                                    (MobileAppTrackingTypeEnum)src.MobileAppTrackingType :
                                                                    MobileAppTrackingTypeEnum.InAppBrowser,
                Notification = src.NotificationMsg,
                ApprovalTime = src.ApprovalTime,
                BasicTerms = src.BasicTerms,
                SpecialTerms = src.ExtentedTerms,
                CashbackGuidelines = src.CashbackGuideLine,
                Channel = MerchantProfile.GetChannelName(src),
                Confirmation = src.ConfirmationMsg,
                TrackingTime = src.TrackingTime,
                ClientCommission = src.ClientCommission,
                ClientCommissionSummary = force2DecimalCommissionString ? GetClientCommissionStringWith2DecimalPlaces(src) : src.ClientCommissionString,
                ReferenceName = src.ReferenceName,
                MerchantBadgeCode = src.MerchantBadgeCode,
                IsMobileEnabled = src.MobileEnabled ?? false,
                IsDesktopEnabled = src.DesktopEnabled,
                IsMobileAppEnabled = src.IsMobileAppEnabled ?? false,
                BackgroundImageUrl = src.BackgroundImageUrl,
                BannerImageUrl = src.BannerImageUrl,
                MobileTrackingNetwork = src.MobileTrackingNetwork.HasValue && System.Enum.IsDefined(typeof(MobileTrackingNetworkEnum), src.MobileTrackingNetwork) ?
                                                                    (MobileTrackingNetworkEnum)src.MobileTrackingNetwork :
                                                                    MobileTrackingNetworkEnum.Button,
                IsGiftCard = src.NetworkId == Networks.TrueRewards,                
                IsPaused= src.IsPaused
            };
        }

        public MerchantStore.Tier ConvertToMerchantStoreTier(MerchantTier merchantTier, bool force2DecimalCommissionString = false)
        {
            if (merchantTier == null)
                return null;

            var src = merchantTier;
            return new MerchantStore.Tier
            {
                Id = src.MerchantTierId,
                Name = src.TierDescription,
                ClientCommissionString = force2DecimalCommissionString ? GetClientCommissionStringWith2DecimalPlaces(merchantTier) : merchantTier.ClientCommissionString,
                ClientCommission = src.ClientCommission,
                CommissionType = MerchantProfile.CommissionTypeDict[src.TierCommTypeId],
                EndDateTime = src.EndDate.ToString("s"),
                Terms = src.TierSpecialTerms,
                Exclusions = src.TierExclusions,
                TierLinks = src.TierLinks,
                TierReference = src.TierReference,
                TierSpecialTerms = src.TierSpecialTerms,
                Ranking = src.Ranking,
                IsFeatured = src.IsFeatured,
                DescriptionLong = src.TierDescriptionLong,
                TierImageUrl = src.TierImageUrl,
                BadgeCode = src.BadgeCode,
                TrackingLink = src.TrackingLink,
                Premium = src.Premium != null ? new PremiumTier()
                {
                    ClientCommission = src.Premium.ClientCommission,
                    CommissionType = src.Premium.CommissionType,
                    ClientCommissionString = src.Premium.ClientCommissionString
                } : null
            };
        }

        public OfflineMerchantStore ConvertToOfflineMerchantStore(MerchantFullView merchantFullView, bool force2DecimalCommissionString = false)
        {
            if (merchantFullView == null)
                return null;

            var src = merchantFullView;
            return new OfflineMerchantStore
            {
                MerchantId = src.MerchantId,
                Name = src.MerchantName,
                HyphenatedString = src.HyphenatedString,
                LogoUrl = src.RegularImageUrl,
                DescriptionShort = src.DescriptionShort,
                DescriptionLong = src.DescriptionLong,
                IsFlatRate = src.IsFlatRate ?? false,
                CommissionType = MerchantProfile.CommissionTypeDict[src.TierCommTypeId],
                TierTypeId = src.TierTypeId,
                MobileAppTrackingType = src.MobileAppTrackingType.HasValue && System.Enum.IsDefined(typeof(MobileAppTrackingTypeEnum), src.MobileAppTrackingType) ?
                                                                    (MobileAppTrackingTypeEnum)src.MobileAppTrackingType :
                                                                    MobileAppTrackingTypeEnum.InAppBrowser,
                Notification = src.NotificationMsg,
                ApprovalTime = src.ApprovalTime,
                BasicTerms = src.BasicTerms,
                SpecialTerms = src.ExtentedTerms,
                CashbackGuidelines = src.CashbackGuideLine,
                Channel = MerchantProfile.GetChannelName(src),
                Confirmation = src.ConfirmationMsg,
                TrackingTime = src.TrackingTime,
                ClientCommission = src.ClientCommission,
                ClientCommissionSummary = force2DecimalCommissionString ? GetClientCommissionStringWith2DecimalPlaces(src) : src.ClientCommissionString,
                ReferenceName = src.ReferenceName,
                MerchantBadgeCode = src.MerchantBadgeCode,
                IsMobileEnabled = src.MobileEnabled ?? false,
                IsDesktopEnabled = src.DesktopEnabled,
                IsMobileAppEnabled = src.IsMobileAppEnabled ?? false,
                BackgroundImageUrl = src.BackgroundImageUrl,
                BannerImageUrl = src.BannerImageUrl,
                MobileTrackingNetwork = src.MobileTrackingNetwork.HasValue && System.Enum.IsDefined(typeof(MobileTrackingNetworkEnum), src.MobileTrackingNetwork) ?
                                                                    (MobileTrackingNetworkEnum)src.MobileTrackingNetwork :
                                                                    MobileTrackingNetworkEnum.Button,
                IsGiftCard = src.ClientId == Networks.TrueRewards
            };
        }

        public MerchantStorePremium ConvertToPremiumMerchant(MerchantFullView merchantFullView, bool force2DecimalCommissionString = false)
        {
            var src = merchantFullView;
            return new MerchantStorePremium()
            {
                ClientCommissionSummary = force2DecimalCommissionString ? GetClientCommissionStringWith2DecimalPlaces(merchantFullView) : src.ClientCommissionString,
                ClientCommission = src.ClientCommission,
                IsFlatRate = src.IsFlatRate ?? false,
                Notification = src.NotificationMsg,
                Confirmation = src.ConfirmationMsg,
                TrackingTime = src.TrackingTime,
                ApprovalTime = src.ApprovalTime
            };
        }

        public IEnumerable<MerchantDto> ConvertToMerchantDto(IEnumerable<MerchantViewModel> merchants)
        {
            return merchants.
                Select(x => new MerchantDto
                {
                    Id = x.MerchantId,
                    Name = x.MerchantName,
                    HyphenatedString = x.HyphenatedString,
                    LogoUrl = x.RegularImageUrl,
                    Description = x.DescriptionShort,
                    Commission = Math.Round((x.Commission * x.ClientComm * x.MemberComm) / 10000, 2),
                    IsFlatRate = (bool)x.IsFlatRate,
                    CommissionType = MerchantProfile.GetMerchantCommissionTypeString(x.TierCommTypeId),
                    TrackingTimeMin = MerchantProfile.GetTrackingTimeMin(x.TrackingTime),
                    TrackingTimeMax = MerchantProfile.GetTrackingTimeMax(x.TrackingTime),
                    ApprovalTime = MerchantProfile.GetApprovalTime(x.ApprovalTime),
                    SpecialTerms = x.ExtentedTerms,
                    CashbackGuidelines = x.BasicTerms,
                    OfferCount = (decimal)x.OfferCount,
                    RewardType = MerchantProfile.GetRewardsType(x.TierTypeId),
                    MobileTrackingType = x.MobileAppTrackingType.GetEnumDescription(),
                    MerchantBadge = x.MerchantBadgeCode,
                    IsCustomTracking = MerchantProfile.IsCustomTracking(x.MerchantId, CustomTrackingMerchantList),
                    DesktopEnabled = x.DesktopEnabled,
                    MobileTrackingNetwork = x.MobileAppTrackingType == Enum.MobileAppTrackingTypeEnum.AppToApp ? x.MobileTrackingNetwork.GetEnumDescription() : string.Empty,
                    BackgroundImageUrl = x.BackgroundImageUrl,
                    BannerImageUrl = x.BannerImageUrl,
                    ClientCommissionString = x.ClientCommissionString,
                    Premium = x.Premium,
                    IsPaused = x.IsPaused
                }).ToList();
        }

        public MerchantTier ConvertToMerchantTierDto(MerchantTierView merchantTierView)
        {
            if (merchantTierView == null)
                return null;

            var src = merchantTierView;
            return new MerchantTier
            {
                MerchantTierId = src.MerchantTierId,
                MerchantId = src.MerchantId,
                TierDescription = src.TierDescription,
                Commission = src.Commission,
                ClientComm = src.ClientComm,
                MemberComm = src.MemberComm,
                TierCommTypeId = src.TierCommTypeId,
                EndDate = src.EndDate,
                TierImageUrl = src.TierImageUrl,
                TierDescriptionLong = src.TierDescriptionLong,
                TrackingLink = src.TrackingLink,
                TierSpecialTerms = src.TierSpecialTerms,
                TierExclusions = src.TierExclusions,
                TierReference = src.TierReference,
                IsFeatured = src.IsFeatured,
                Ranking = src.Ranking
            };
        }

        public MerchantTier ConvertToMerchantTierDtoWithBadgeCode(MerchantTierViewWithBadge merchantTierView)
        {
            var tier = ConvertToMerchantTierDto(merchantTierView);
            if(tier != null)
            {
                tier.BadgeCode = merchantTierView.BadgeCode;
            }
            return tier;
        }

        public MerchantTierLink ConvertToMerchantTierLink(MerchantTierLinkModel merchantTierLink)
        {
            var src = merchantTierLink;
            return new MerchantTierLink
            {
                MerchantTierLinkId = src.MerchantTierLinkId,
                MerchantTierId = src.MerchantTierId,
                MerchantTierLinkName = src.MerchantTierLinkName,
                MerchantTierLinkUrl = src.MerchantTierLinkUrl
            };
        }

        public MerchantStore.Offer ConvertToOffer(OfferViewModel offer, int? premiumClientId = null, bool force2DecimalCommissionString = false)
        {
            if (offer == null)
                return null;

            var src = offer;
            return new MerchantStore.Offer
            {
                Id = src.OfferId,
                EndDateTime = src.DateEnd.ToString("s"),
                HyphenatedString = src.HyphenatedString,
                Title = src.OfferTitle,
                Description = src.OfferDescription,
                ClientCommissionString = force2DecimalCommissionString ? GetClientCommissionStringWith2DecimalPlaces(src) : src.ClientCommissionString,
                CouponCode = src.CouponCode,
                MerchantLogoUrl = src.RegularImageUrl,
                MerchantId = src.MerchantId,
                Terms = src.OfferTerms,
                OfferBadgeCode = src.OfferBadgeCode,
                OfferBackgroundImageUrl = src.OfferBackgroundImageUrl,
                IsCashbackIncreased = src.IsCashbackIncreased,
                IsPremium = src.ClientId == premiumClientId,
                IsPremiumFeature = src.IsPremiumFeature,
                IsFeatured = src.IsFeatured
            };
        }

        public MerchantBundleDetailResultModel ConvertToMerchantBundleDetailResultModel(MerchantStoresBundle merchantStoresBundles)
        {
            var src = merchantStoresBundles;
            return new MerchantBundleDetailResultModel
            {
                Online = src.OnlineStore != null ? ConvertToMerchantBundleDetailResultModelOnline(src.OnlineStore) : null,
                Offline = src.OfflineStores != null ?
                                                    src.OfflineStores.Select(offlineStore => ConvertToMerchantBundleDetailResultModelOffline(offlineStore)).ToList()
                                                    : null
            };
        }

        private CardLinkedMerchantModel ConvertToMerchantBundleDetailResultModelOnline(MerchantStore OnlineStore)
        {
            var src = OnlineStore;
            return new CardLinkedMerchantModel
            {
                Id = src.MerchantId,
                Offers = src.Offers.Select(offer => ConvertToOfferModel(offer)).ToList(),
                MerchantTiers = src.Tiers.Select(tier => _mapper.Map<MerchantTierResultModel>(tier)).ToList(),
                Name = src.Name,
                HyphenatedString = src.HyphenatedString,
                LogoUrl = src.LogoUrl,
                Description = src.DescriptionLong,
                Commission = src.ClientCommission,
                IsFlatRate = src.IsFlatRate,
                CommissionType = src.CommissionType,
                CommissionString = src.ClientCommissionSummary,
                TrackingTimeMin = MerchantProfile.GetTrackingTimeMin(src.TrackingTime),
                TrackingTimeMax = MerchantProfile.GetTrackingTimeMax(src.TrackingTime),
                ApprovalTime = MerchantProfile.GetApprovalTime(src.ApprovalTime),
                SpecialTerms = src.SpecialTerms,
                CashbackGuidelines = src.CashbackGuidelines,
                OfferCount = src.OfferCount,
                RewardType = MerchantProfile.GetRewardsType(src.TierTypeId),
                MobileTrackingType = src.MobileAppTrackingType.GetEnumDescription(),
                IsCustomTracking = MerchantProfile.IsCustomTracking(src.MerchantId, CustomTrackingMerchantList),
                BackgroundImageUrl = src.BackgroundImageUrl,
                BannerImageUrl = src.BannerImageUrl,
                MerchantBadge = src.MerchantBadgeCode,
                DesktopEnabled = src.IsDesktopEnabled,
                MobileTrackingNetwork = src.MobileAppTrackingType == Enum.MobileAppTrackingTypeEnum.AppToApp ? src.MobileTrackingNetwork.GetEnumDescription() : string.Empty,
                Premium = _mapper.Map<MerchantBasicPremiumModel>(OnlineStore.Premium),
                IsPaused = src.IsPaused
            };
            
        }

        private InStoreMerchantModel ConvertToMerchantBundleDetailResultModelOffline(OfflineMerchantStore offlineMerchantStore)
        {
            var src = offlineMerchantStore;
            return new InStoreMerchantModel
            {
                Stores = src.Stores,
                VisaMeta = MerchantProfile.GetVisaMetaData(src),
                Offers = src.Offers != null ? src.Offers.Select(offer => ConvertToOfferModel(offer)).ToList() : null,
                MerchantTiers = src.Tiers != null ? src.Tiers.Select(tier => _mapper.Map<MerchantTierResultModel>(tier)).ToList() : null,
                Id = src.MerchantId,
                Name = src.Name,
                HyphenatedString = src.HyphenatedString,
                LogoUrl = src.LogoUrl,
                Description = src.DescriptionLong,
                Commission = src.ClientCommission,
                IsFlatRate = src.IsFlatRate,
                CommissionType = src.CommissionType,
                CommissionString = src.ClientCommissionSummary,
                TrackingTimeMin = MerchantProfile.GetTrackingTimeMin(src.TrackingTime),
                TrackingTimeMax = MerchantProfile.GetTrackingTimeMax(src.TrackingTime),
                ApprovalTime = MerchantProfile.GetApprovalTime(src.ApprovalTime),
                SpecialTerms = src.SpecialTerms,
                CashbackGuidelines = src.CashbackGuidelines,
                OfferCount = src.OfferCount,
                RewardType = MerchantProfile.GetRewardsType(src.TierTypeId),
                MobileTrackingType = src.MobileAppTrackingType.GetEnumDescription(),
                IsCustomTracking = MerchantProfile.IsCustomTracking(src.MerchantId, CustomTrackingMerchantList),
                BackgroundImageUrl = src.BackgroundImageUrl,
                BannerImageUrl = src.BannerImageUrl,
                MerchantBadge = src.MerchantBadgeCode,
                DesktopEnabled = src.IsDesktopEnabled,
                MobileTrackingNetwork = src.MobileAppTrackingType == Enum.MobileAppTrackingTypeEnum.AppToApp ? src.MobileTrackingNetwork.GetEnumDescription() : string.Empty,
                Premium = _mapper.Map<MerchantBasicPremiumModel>(src.Premium)
            };
        }

        public OfflineMerchantStore.Store ConvertToOfflineStore(StoreModel store)
        {
            var src = store;
            return new OfflineMerchantStore.Store
            {
                StoreId = src.StoreId,
                Name = src.Name,
                DisplayName = src.DisplayName,
                Description = src.Description,
                Address = src.Address,
                Suburb = src.Suburb,
                PostCode = src.PostCode,
                State = StateIdDict[src.StateId ?? 1] ?? string.Empty,
                Longitude = src.Longitude == null ? string.Empty : src.Longitude.ToString(),
                Latitude = src.Latitudes == null ? string.Empty : src.Latitudes.ToString()
            };
        }

        public OfferModel ConvertToOfferModel(MerchantStore.Offer offer)
        {
            var src = offer;
            return new OfferModel
            {
                Id = src.Id,
                Title = src.Title,
                CouponCode = src.CouponCode,
                EndDateTime = src.EndDateTime,
                Description = src.Description,
                HyphenatedString = src.HyphenatedString,
                IsFeatured = src.IsFeatured,
                Terms = src.Terms,
                MerchantId = src.MerchantId,
                MerchantLogoUrl = src.MerchantLogoUrl,
                OfferBackgroundImageUrl = src.OfferBackgroundImageUrl,
                OfferBadge = src.OfferBadgeCode,
                IsCashbackIncreased = src.IsCashbackIncreased,
                IsPremiumFeature = src.IsPremiumFeature
            };

        }


        public IEnumerable<AllStoresMerchantModel> ConvertToAllStoresMerchantModel(IEnumerable<MerchantStoresBundle> merchantStoresBundles, IEnumerable<MerchantFullView> premiumMerchants)
        {
            return merchantStoresBundles.Select(msb =>
            {
            try
            {
                var listWithOnlineStore = msb.OnlineStore != null ? new List<MerchantStore>() { msb.OnlineStore } : new List<MerchantStore>();
                var stores = msb.OfflineStores != null ? msb.OfflineStores.Concat(listWithOnlineStore) : listWithOnlineStore;

                string commissionString;

                if (StoresHaveSameCommissionTypes(stores))
                {
                    var isFlatRate = MerchantBundleIsFlatRate(stores);
                    var maxCommission = stores.Select(s => s.ClientCommission).Max();
                    var commissionType = stores.First().CommissionType;
                    commissionString = GetCommissionString(isFlatRate, maxCommission, commissionType);
                }
                else
                {
                    var maxPercentCommission = stores
                        .Where(s => s.CommissionType == Constants.MerchantCommissionType.Percent)
                        .Select(s => s.ClientCommission)
                        .Max();
                    commissionString = GetCommissionString(false, maxPercentCommission, Constants.MerchantCommissionType.Percent);
                }

                var mainMerchant = GetMainMerchant(msb);

                return new AllStoresMerchantModel()
                {
                    Id = mainMerchant.MerchantId,
                    Name = mainMerchant.Name,
                    HyphenatedString = mainMerchant.HyphenatedString,
                    ClientCommission = mainMerchant.ClientCommission,
                    CommissionString = commissionString,
                    InStore = msb.OfflineStores != null && msb.OfflineStores.Any(),
                    Online = msb.OnlineStore != null,
                    LogoUrl = mainMerchant.LogoUrl,
                    Premium = premiumMerchants?.Where(pm => pm.MerchantId == mainMerchant.MerchantId)
                                            .Select(merchant => new PremiumDto()
                                            { 
                                                Commission = merchant.ClientCommission, 
                                                IsFlatRate = (bool)merchant.IsFlatRate, 
                                                ClientCommissionString = merchant.ClientCommissionString })
                                            .FirstOrDefault()
                                            };
                }
                catch (Exception e)
                {
                    Log.Error("Error mapping to all store merchant: " + e.ToString());
                    return null;
                }

            });
        }

        public PremiumTier ConvertToPremiumTier(MerchantTier merchantTier, bool force2DecimalCommissionString = false)
        {
            return new PremiumTier()
            {
                ClientCommission = merchantTier.ClientCommission,
                CommissionType = CommissionTypeDict[merchantTier.TierCommTypeId],
                ClientCommissionString = force2DecimalCommissionString ? GetClientCommissionStringWith2DecimalPlaces(merchantTier) : merchantTier.ClientCommissionString
            };
        }

        private MerchantStore GetMainMerchant(MerchantStoresBundle merchantStoresBundle)
        {
            if (merchantStoresBundle.OnlineStore != null)
            {
                return merchantStoresBundle.OnlineStore;
            }

            return merchantStoresBundle.OfflineStores.First();
        }

        private bool MerchantBundleIsFlatRate(IEnumerable<MerchantStore> stores)
        {
            var firstCommission = stores.First().ClientCommission;
            return stores.All(s => (s.ClientCommission == firstCommission) && s.IsFlatRate);
        }

        private string GetCommissionString(bool isFlatRate, decimal commission, string commissionType)
        {
            if (commission == 0)
            {
                return "No current offers";
            }
            var roundedCommission = commission.RoundToTwoDecimalPlaces();

            var prefix = isFlatRate ? "" : "Up to ";

            string commissionNumberAsString;
            if (commissionType == Constants.MerchantCommissionType.Dollar)
            {
                commissionNumberAsString = string.Format(roundedCommission % 1 == 0 ? "{0:0}" : "{0:0.00}", roundedCommission);
            }
            else
            {
                commissionNumberAsString = roundedCommission.ToString("0.##");
            }

            var commissionValue = commissionType == Constants.MerchantCommissionType.Dollar ? $"${commissionNumberAsString}" : $"{commissionNumberAsString}%";

            return $"{prefix}{commissionValue} cashback";
        }

        private bool StoresHaveSameCommissionTypes(IEnumerable<MerchantStore> stores)
        {
            return stores.All(s => s.CommissionType == Constants.MerchantCommissionType.Dollar) || stores.All(s => s.CommissionType == Constants.MerchantCommissionType.Percent);
        }

        private string GetClientCommissionStringWith2DecimalPlaces(MerchantTier src)
        {
            return MerchantTierSummaryBase.GetTierCommissionString(src.ClientCommission, src.TierCommTypeId, true);
        }

        private string GetClientCommissionStringWith2DecimalPlaces(OfferViewModel src)
        {
            return MerchantTierSummaryBase.GetCommissionString(src.ClientProgramTypeId, src.TierCommTypeId, src.ClientCommission, src.Rate, src.IsFlatRate, src.TierTypeId, src.RewardName);
        }

        private string GetClientCommissionStringWith2DecimalPlaces(MerchantFullView src)
        {
            return MerchantTierSummaryBase.GetCommissionString(src.ClientProgramTypeId, src.TierCommTypeId, src.ClientCommission, src.Rate, src.IsFlatRate, src.TierTypeId, src.RewardName, true);
        }
    }
}
