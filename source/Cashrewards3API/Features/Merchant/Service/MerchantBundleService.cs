using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.Merchant.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.Merchant.Repository;
using static Cashrewards3API.Common.Constants;
using Cashrewards3API.Features.Offers;
using Cashrewards3API.Enum;
using Cashrewards3API.Common.Services.Interfaces;

namespace Cashrewards3API.Features.Merchant
{
    public interface IMerchantBundleService
    {
        Task<MerchantBundleDetailResultModel> GetMerchantBundleByIdAsync(int clientId, int merchantId, int? premiumClientId, bool isMobile, IncludePremiumEnum includePremium = IncludePremiumEnum.Preview);

        Task<MerchantStoresBundle> GetMerchantStoresBundleByHyphenatedString(int clientId, int? premiumClientId, string hyphenatedString, bool isMobile);
    }

    public class MerchantBundleService : IMerchantBundleService
    {
        private readonly IConfiguration Configuration;
        private readonly IMerchantMappingService merchantMappingService;
        private readonly ILogger<MerchantBundleService> logger;
        private readonly IRedisUtil redisUtil;
        private readonly CacheConfig cacheConfig;
        private readonly INetworkExtension _networkExtension;
        private readonly IMerchantRepository merchantRepository;
        private readonly IPremiumService premiumService;
        private readonly ICacheKey cacheKey;
        private readonly IFeatureToggle _featureToggle;


        public MerchantBundleService(IConfiguration configuration,
                                   IMerchantMappingService merchantMappingService,
                                   ILogger<MerchantBundleService> logger,
                                   IRedisUtil redisUtil,
                                   ICacheKey cacheKey,
                                   CacheConfig cacheConfig,
                                   INetworkExtension networkExtension,
                                   IMerchantRepository merchantRepository,
                                   IPremiumService premiumService,
                                   IFeatureToggle featureToggle)
        {
            Configuration = configuration;
            this.merchantMappingService = merchantMappingService;
            this.logger = logger;
            this.redisUtil = redisUtil;
            this.cacheConfig = cacheConfig;
            _networkExtension = networkExtension;
            this.merchantRepository = merchantRepository;
            this.premiumService = premiumService;
            this.cacheKey = cacheKey;
            this._featureToggle = featureToggle;
        }

        public async Task<MerchantBundleDetailResultModel> GetMerchantBundleByIdAsync(int clientId, int merchantId, int? premiumClientId = null, bool isMobile = false, IncludePremiumEnum includePremium = IncludePremiumEnum.Preview)
        {
            string key = cacheKey.GetMerchantBundleByMerchantIdKey(clientId, merchantId, premiumClientId, isMobile, includePremium);
            return await redisUtil.GetDataAsyncWithEarlyRefresh(key,
                () => GetMerchantBundleByIdFromDbAsync(clientId, merchantId, premiumClientId, isMobile, includePremium),
                                        cacheConfig.MerchantDataExpiry);
        }

        public async Task<MerchantStoresBundle> GetMerchantStoresBundleByHyphenatedString(int clientId, int? premiumClientId, string hyphenatedString, bool isMobile)
        {
            string key = cacheKey.GetMerchantBundleByHyphenatedStringKey(clientId, hyphenatedString, premiumClientId, isMobile);
            return await redisUtil.GetDataAsyncWithEarlyRefresh(key,
                () => GetMerchantStoresBundleByHyphenatedStringFromDbAsync(clientId, premiumClientId, hyphenatedString, isMobile),
                cacheConfig.MerchantDataExpiry);
        }

        private async Task<MerchantBundleDetailResultModel> GetMerchantBundleByIdFromDbAsync(int clientId, int merchantId, int? premiumClientId, bool isMobile, IncludePremiumEnum includePremium = IncludePremiumEnum.Preview)
        {
          
            var merchant = await merchantRepository.GetMerchantByClientIdAsync(clientId, merchantId);
            if(premiumClientId.HasValue && merchant !=null)
            {
                merchant = MerchantHelpers.ExcludePremiumDisabledMerchants(new List<MerchantFullView> { merchant}).FirstOrDefault();
            }

            MerchantBundleDetailResultModel result = null;
            if (merchant != null)
            {
                includePremium = premiumClientId.HasValue ? IncludePremiumEnum.All : includePremium;

                premiumClientId ??= premiumService.GetPremiumClientId(clientId);

                var merchantStoreBundle = await GetMerchantStoresBundleAsync(clientId, premiumClientId, merchant.HyphenatedString, includePremium, isMobile);

                if (_featureToggle.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED) && merchantStoreBundle.OnlineStore != null)
                    MerchantHelpers.ForceNoCommissionWhenPausedMerchant(merchantStoreBundle.OnlineStore);

                result = merchantMappingService.ConvertToMerchantBundleDetailResultModel(merchantStoreBundle);
            }

            return result;
        }

        private async Task<MerchantStoresBundle> GetMerchantStoresBundleByHyphenatedStringFromDbAsync(int clientId, int? premiumClientId, string hyphenatedString, bool isMobile)
        {
            var includePremium = premiumClientId.HasValue ? IncludePremiumEnum.All : IncludePremiumEnum.Preview;

            premiumClientId ??= premiumService.GetPremiumClientId(clientId);

            var bundles = await GetMerchantStoresBundleAsync(clientId, premiumClientId, hyphenatedString, includePremium, isMobile);

            if (_featureToggle.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED) && bundles.OnlineStore != null && bundles.OnlineStore.IsPaused)
            {
                bundles.OnlineStore.ClientCommission = 0;
                bundles.OnlineStore.ClientCommissionSummary = string.Empty;
                if (bundles.OnlineStore.Premium != null)
                {
                    bundles.OnlineStore.Premium.ClientCommission = 0;
                    bundles.OnlineStore.Premium.ClientCommissionSummary = string.Empty;
                }

                bundles.OnlineStore.Tiers.ForEach(p =>
                {
                    p.ClientCommission = 0;
                    p.ClientCommissionString = string.Empty;
                    if (p.Premium != null)
                    {
                        p.Premium.ClientCommission = 0;
                        p.Premium.ClientCommissionString = string.Empty;
                    }
                });

                bundles.OnlineStore.Offers.Clear();

            }
            return bundles;
        }


        private async Task<MerchantStoresBundle> GetMerchantStoresBundleAsync(int clientId, int? premiumClientId, string hyphenatedString, IncludePremiumEnum includePremium, bool isMobile = false)
        {
            var clientIds = new List<int>() { clientId };

            if((includePremium == IncludePremiumEnum.All || includePremium == IncludePremiumEnum.Preview) && premiumClientId.HasValue)
            {
                clientIds.Add(premiumClientId.Value);
            }

            var merchants = (await merchantRepository.GetMerchantsByClientIdsAndHyphenatedStringAsync(clientIds, hyphenatedString)).ToList();
            if (premiumClientId.HasValue && includePremium == IncludePremiumEnum.All)
            {
                merchants = MerchantHelpers.ExcludePremiumDisabledMerchants(merchants).ToList();
            }

            var regularMerchants = merchants.Where(m => m.ClientId == clientId).ToList();
            var premiumMerchants = merchants.Where(m => m.ClientId == premiumClientId).ToList();

            var merchantStoreBundle = new MerchantStoresBundle();
            if (regularMerchants.Any())
            {
                MerchantFullView onlineStore;
                if (isMobile)
                {
                    var buttonRelatedStore = regularMerchants.FirstOrDefault(m => _networkExtension.IsInMobileSpecificNetwork(m.NetworkId));
                    onlineStore = buttonRelatedStore != null ? buttonRelatedStore : regularMerchants.FirstOrDefault(m => !_networkExtension.IsInStoreNetwork(m.NetworkId));
                }
                else
                {
                    onlineStore = regularMerchants.FirstOrDefault(m => !_networkExtension.IsInStoreNetwork(m.NetworkId) && !_networkExtension.IsInMobileSpecificNetwork(m.NetworkId));
                }

                var premiumClientIdForTiers = includePremium == IncludePremiumEnum.All ? premiumClientId : null;
                var premiumClientIdForOffers = includePremium == IncludePremiumEnum.All ? premiumClientId : null;


                if (onlineStore != null && (!isMobile || (isMobile && (!onlineStore.IsMobileAppEnabled.HasValue || onlineStore.IsMobileAppEnabled == true))))
                {
                    merchantStoreBundle.OnlineStore = merchantMappingService.ConvertToMerchantStore(onlineStore, false);

                    merchantStoreBundle.OnlineStore.Tiers = await GetMerchantTiersAsync(clientId, premiumClientIdForTiers, onlineStore.MerchantId);
                    merchantStoreBundle.OnlineStore.Offers = await GetMerchantOffersAsync(clientId, premiumClientIdForOffers, onlineStore.MerchantId);
                    merchantStoreBundle.OnlineStore.Premium = GetPremiumMerchant(clientId, premiumMerchants, onlineStore);

                    if (_networkExtension.IsInCardLinkedNetwork(onlineStore.NetworkId))
                    {
                        PopulateCardLinkedOfferTermsForOnlineStore(merchantStoreBundle.OnlineStore);
                    }
                }

                var offlineStores = regularMerchants.Where(m => _networkExtension.IsInStoreNetwork(m.NetworkId));
                if (offlineStores.Any())
                {
                    merchantStoreBundle.OfflineStores = new List<OfflineMerchantStore>();
                    foreach (var offlineMerchant in offlineStores)
                    {
                        if (!isMobile || (isMobile && (!offlineMerchant.IsMobileAppEnabled.HasValue || offlineMerchant.IsMobileAppEnabled == true)))
                        {
                            var offlineMerchantStore = merchantMappingService.ConvertToOfflineMerchantStore(offlineMerchant, false);
                            var merchantStores = await merchantRepository.GetMerchantStores(offlineMerchantStore.MerchantId);
                            offlineMerchantStore.Stores = merchantStores.Select(
                                                            store => merchantMappingService.ConvertToOfflineStore(store))
                                                            .ToList();
                            offlineMerchantStore.Tiers = await GetMerchantTiersAsync(clientId, premiumClientIdForTiers, offlineMerchantStore.MerchantId);
                            offlineMerchantStore.Premium = GetPremiumMerchant(clientId, premiumMerchants, offlineMerchant);
                            if (_networkExtension.IsInCardLinkedNetwork(offlineMerchant.NetworkId))
                            {
                                PopulateCardLinkedOfferTermsForOfflineStore(offlineMerchantStore);
                            }

                            merchantStoreBundle.OfflineStores.Add(offlineMerchantStore);
                        }
                    }
                }
            }
            return merchantStoreBundle;
        }

        private MerchantStorePremium GetPremiumMerchant(int clientId, IEnumerable<MerchantFullView> premiumMerchants, MerchantFullView merchant)
        {
            var premiumMerchant = premiumMerchants.FirstOrDefault(m => m.MerchantId == merchant.MerchantId);

            if (premiumMerchant == null || clientId != Constants.Clients.CashRewards)
                return null;

            return merchantMappingService.ConvertToPremiumMerchant(premiumMerchant, false);
        }

        private async Task<List<MerchantStore.Offer>> GetMerchantOffersAsync(int clientId, int? premiumClientId, int merchantId)
        {
            var clientIds = new List<int>() { clientId };
            if (premiumClientId.HasValue)
                clientIds.Add(premiumClientId.Value);

            var groupedOfferViews = (await merchantRepository.GetMerchantOfferViewsAsync(clientIds, merchantId)).GroupBy(o => o.OfferId);


            IEnumerable<OfferViewModel> consolidatedOfferViews;
            if (premiumClientId.HasValue)
            {
                consolidatedOfferViews = groupedOfferViews.Select(offerGrouping => offerGrouping.FirstOrDefault(o => o.ClientId == premiumClientId) ?? offerGrouping.First());
            } 
            else
            {
                consolidatedOfferViews = groupedOfferViews.Where(offerGrouping => offerGrouping.Any(o => o.ClientId == clientId)).Select(offerGrouping => offerGrouping.Single(o => o.ClientId == clientId));
            }
            

            consolidatedOfferViews = consolidatedOfferViews.OrderByDescending(offer => offer.IsFeatured)
                .ThenByDescending(offer => offer.Ranking)
                .ThenBy(offer => offer.DateEnd);

            var offers = consolidatedOfferViews.Select(offer => merchantMappingService.ConvertToOffer(offer, premiumClientId, true)).ToList();

            return offers;
           
        }

        private void PopulateCardLinkedOfferTermsForOnlineStore(MerchantStore merchantStore)
        {
            if (merchantStore != null && merchantStore.Tiers != null)
            {
                merchantStore.CardLinkedOfferTerms = merchantStore.Tiers.FirstOrDefault()?.TierSpecialTerms;
            }
        }
        private void PopulateCardLinkedOfferTermsForOfflineStore(OfflineMerchantStore merchantStore)
        {
            if (merchantStore != null && merchantStore.Tiers != null)
            {
                merchantStore.CardLinkedOfferTerms = merchantStore.Tiers.FirstOrDefault()?.TierSpecialTerms;
            }
        }

        private async Task<List<MerchantStore.Tier>> GetMerchantTiersAsync(int clientId, int? premiumClientId, int merchantId)
        {
            var clientIds = new List<int>() { clientId };
            if (premiumClientId.HasValue)
            {
                clientIds.Add(premiumClientId.Value);
            }

            var merchantTierViews = await merchantRepository.GetMerchantTierViewsWithBadgeAsync(clientIds, merchantId);
            merchantTierViews = merchantTierViews.Where(mv => mv.TierTypeId != (int)TierTypeEnum.Hidden);
            var regularTierViews = merchantTierViews.Where(t => t.ClientId == clientId);
            var premiumTierViews = merchantTierViews.Where(t => t.ClientId == premiumClientId);
           

            IEnumerable<MerchantTier> generalTiers = regularTierViews
                 .Select(merchantTierView => merchantMappingService.ConvertToMerchantTierDtoWithBadgeCode(merchantTierView)).ToList(); ;
            var generalPremiumTiers = premiumTierViews
                 .Select(merchantTierView => merchantMappingService.ConvertToMerchantTierDtoWithBadgeCode(merchantTierView)).ToList(); ;

            foreach (var tier in generalTiers)
            {
                var tierLinks = await merchantRepository.GetMerchantTierLinks(tier.MerchantTierId);
                var merchantTierLinks = tierLinks.Select(tierLink =>
                                                    merchantMappingService.ConvertToMerchantTierLink(tierLink));

                tier.TierLinks = merchantTierLinks.ToList();
                var merchantTier = await merchantRepository.GetMerchantTierModel(tier.MerchantTierId);
                tier.TierReference = merchantTier?.TierReference;

                var premiumTier = generalPremiumTiers.SingleOrDefault(premiumTier => premiumTier.MerchantTierId == tier.MerchantTierId);
                if (premiumTier != null)
                {
                    tier.Premium = merchantMappingService.ConvertToPremiumTier(premiumTier, false);
                }
            }

            var merchantStoreTiers = generalTiers.Select(merchantTier =>
                                                merchantMappingService.ConvertToMerchantStoreTier(merchantTier, false));

            return merchantStoreTiers.OrderByDescending(t => t.ClientCommission).ToList();
        }



    }
}
