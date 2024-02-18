using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Member.Models;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Merchant.Repository;
using Cashrewards3API.Features.Offers;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace Cashrewards3API.Features.Merchant
{
    public interface IMerchantService
    {
        Task<PagedList<MerchantSearchResponseModel>> GetMerchantsSearchByFilter(
            MerchantByFilterRequestModel merchantByFilterRequestModel);
       
        Task<PagedList<CmsTrackingMerchantSearchResonseModel>> GetCmsTrackingMerchantsSearchByFilter(
            CmsTrackingMerchantSearchFilterRequestModel fitler);

        Task<IList<MerchantTier>> GetMerchantTiers(int merchantId, string tierName, DateTime startDateUtc,
            DateTime endDateUtc, SqlConnection conn, SqlTransaction transaction);

        Task<MerchantTier> GetMerchantTierById(int merchantTierId, SqlConnection conn, SqlTransaction transaction);

        Task<MerchantSearchResponseModel> GetMerchantsSearchById(int id, int clientId);

        Task<PagedList<MerchantBundleBasicModel>> GetMerchantBundleByFilterAsync(
            MerchantRequestInfoModel merchantRequestInfoModel);

        Task<TimeZoneInfo> GetTimezoneInfoForMerchant(int merchantId);

        Task<PagedList<AllStoresMerchantModel>> GetAllStoresMerchantsByFilterAsync(
            int clientId, int categoryId, int offset, int limit, int? premiumClientId);

        Task<MerchantCountModel> GetTotalMerchantCountForClient(int clientId);

        Task<IEnumerable<MerchantViewModel>> GetMerchantsForStandardClient
            (int clientId, IEnumerable<int> merchantIds);

        Task<IEnumerable<MerchantViewModel>> GetMerchantsForStandardAndPremiumClients(
            int clientId, int premiumClientId, IEnumerable<int> merchantIds);

        Task<IEnumerable<MerchantViewModel>> GetPremiumDisabledMerchants();

    }

    public class MerchantItem
    {
        public MerchantFullView BaseMerchant { get; set; }

        public MerchantFullView PremiumMerchant { get; set; }

        public MerchantFullView Merchant => BaseMerchant ?? PremiumMerchant;

        public int NetworkId => BaseMerchant?.NetworkId ?? PremiumMerchant.NetworkId;
    }

    public class MerchantService : IMerchantService
    {
        private readonly IMerchantMappingService _merchantMappingService;
        private readonly IRedisUtil _redisUtil;
        private readonly CacheConfig _cacheConfig;
        private readonly INetworkExtension _networkExtension;
        private readonly ICacheKey _cacheKey;
        private readonly IMapper _mapper;
        private readonly IReadOnlyRepository _readOnlyRepository;
        private readonly string _customTrackingMerchantList;
        private readonly MerchantRepository _merchantRepository;
        private readonly IFeatureToggle _featureToggle;

        public MerchantService(
            IConfiguration configuration,
            IMerchantMappingService merchantMappingService,
            IRedisUtil redisUtil,
            ICacheKey cacheKey,
            CacheConfig cacheConfig,
            INetworkExtension networkExtension,
            IMapper mapper,
            IReadOnlyRepository readOnlyRepository,
            IFeatureToggle featureToggle)
        {
            _merchantMappingService = merchantMappingService;
            _redisUtil = redisUtil;
            _cacheConfig = cacheConfig;
            _networkExtension = networkExtension;
            _cacheKey = cacheKey;
            _mapper = mapper;
            _readOnlyRepository = readOnlyRepository;
            _customTrackingMerchantList = configuration["Config:CustomTrackingMerchantList"];
            _merchantRepository = new MerchantRepository(readOnlyRepository);
            _featureToggle = featureToggle;
        }

        public async Task<IList<MerchantTier>> GetMerchantTiers(int merchantId, string tierName, DateTime startDateUtc,
            DateTime endDateUtc, SqlConnection conn, SqlTransaction transaction)
        {
            const string getQuery =
                @"SELECT * FROM MerchantTier WHERE MerchantId=@MerchantId AND TierName=@TierName AND StartDateUtc=@StartDateUtc AND EndDateUtc=@EndDateUtc";
            var merchantTiers = await conn.QueryAsync<MerchantTier>(getQuery, new
            {
                MerchantId = merchantId,
                TierName = tierName,
                StartDateUtc = startDateUtc,
                EndDateUtc = endDateUtc
            }, transaction);

            return merchantTiers.ToList();
        }





        public async Task<PagedList<MerchantSearchResponseModel>> GetMerchantsSearchByFilter(
            MerchantByFilterRequestModel filter)
        {
            var isIdsQuery = filter.Ids.Any();

            var merchants = await GetMerchantsSearchResponseByFilter(filter);
            var merchantSearchResponseModels = merchants as MerchantSearchResponseModel[] ?? merchants.ToArray();
            var totalCount = merchantSearchResponseModels.Count();
            var merchantsData = isIdsQuery
                ? merchantSearchResponseModels
                : merchantSearchResponseModels.Skip(filter.Offset).Take(filter.Limit);
            return new PagedList<MerchantSearchResponseModel>(totalCount, merchantSearchResponseModels.Count(),
                merchantsData.ToList());
        }

        public async Task<PagedList<CmsTrackingMerchantSearchResonseModel>> GetCmsTrackingMerchantsSearchByFilter(
            CmsTrackingMerchantSearchFilterRequestModel filter)
        {
            var merchants = await GetCmsTrackingMerchantsSearchResponseByFilter(filter);
            var merchantSearchResponseModels = merchants as CmsTrackingMerchantSearchResonseModel[] ?? merchants.ToArray();
            var totalCount = merchantSearchResponseModels.Count();
            var merchantsData = merchantSearchResponseModels.Skip(filter.Offset).Take(filter.Limit);
            return new PagedList<CmsTrackingMerchantSearchResonseModel>(totalCount, merchantsData.Count(),
                merchantsData.ToList());
        }

        public async Task<MerchantSearchResponseModel> GetMerchantsSearchById(int id, int clientId)
        {
            return (await GetMerchantsSearchResponseByClientIdAndMerchantIdAsync(clientId, id));
        }

        public async Task<PagedList<MerchantBundleBasicModel>> GetMerchantBundleByFilterAsync(
            MerchantRequestInfoModel merchantRequestInfoModel)
        {
            string key = _cacheKey.GetMerchantBundleByStoreKey(merchantRequestInfoModel);

            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                () => GetMerchantBundleByStoreKey(merchantRequestInfoModel),
                _cacheConfig.MerchantDataExpiry);

        }

        public async Task<PagedList<AllStoresMerchantModel>> GetAllStoresMerchantsByFilterAsync(
            int clientId, int categoryId, int offset, int limit, int? premiumClientId)
        {
            string key = _cacheKey.GetAllStoresMerchantsKey(clientId, categoryId, premiumClientId, offset, limit);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                () => GetAllStoresMerchantsByFilterFromDbAsync(clientId, categoryId, premiumClientId, offset, limit),
                _cacheConfig.MerchantDataExpiry);
        }

        public async Task<MerchantCountModel> GetTotalMerchantCountForClient(int clientId)
        {
            string key = _cacheKey.GetTotalMerchantCountForClientKey(clientId);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                () => GetTotalMerchantCountForClientFromDbAsync(clientId),
                _cacheConfig.MerchantDataExpiry);
        }

        public async Task<PagedList<AllStoresMerchantModel>> GetAllStoresMerchantsByFilterFromDbAsync(
            int clientId, int categoryId, int? premiumClientId, int offset, int limit)
        {
            var uniqueMerchantHyphenatedStrings = await GetOrderedUniqueHyphenatedStringsAsync(
                 clientId, offset, limit, categoryId, MerchantOrderEnum.AlphabeticalNumbersLast);

            if (premiumClientId.HasValue)
            {
                uniqueMerchantHyphenatedStrings = MerchantHelpers.ExcludePremiumDisabledMerchants(uniqueMerchantHyphenatedStrings);
            }

            //TODO: Remove feature flag CPS-68 CPS-1379
            if (_featureToggle.IsEnabled(Common.FeatureFlags.IS_MERCHANT_PAUSED))
            {
                uniqueMerchantHyphenatedStrings = MerchantHelpers.ExcludePausedMerchants(uniqueMerchantHyphenatedStrings);
            }

            var hyphenatedStrings =
                GetKeysForCashbackMerchants(Constants.Clients.CashRewards, uniqueMerchantHyphenatedStrings);

            var totalCount = hyphenatedStrings.Count();
            var pagedHyphenatedStrings = hyphenatedStrings.Skip(offset).Take(limit);

            var merchantFullViewModels =
                await GetMerchantsByClientIdAndUniqueMerchantHyphendatedStringsAsync(
                    new List<int> { clientId },
                    pagedHyphenatedStrings);

            if (premiumClientId.HasValue)
            {
                merchantFullViewModels = MerchantHelpers.ExcludePremiumDisabledMerchants(merchantFullViewModels);
            }

            //TODO: Remove feature flag CPS-68 CPS-1379
            if (_featureToggle.IsEnabled(Common.FeatureFlags.IS_MERCHANT_PAUSED))
            {
                merchantFullViewModels = MerchantHelpers.ExcludePausedMerchants(merchantFullViewModels);
            }

            var merchantStoresBundles = await FormMerchantStoreBundles(new List<int> { clientId }, clientId, premiumClientId, merchantFullViewModels, false);

            merchantStoresBundles = merchantStoresBundles.OrderBy(msb => msb.OnlineStore?.HyphenatedString ?? msb.OfflineStores.FirstOrDefault()?.HyphenatedString, new AlphabeticNumbersLastComparer());
            IEnumerable<MerchantFullView> premiumMerchants = null;
            if (premiumClientId.HasValue)
                premiumMerchants = await GetPremiumMerchants((int)premiumClientId);
            var allStoresModels = _merchantMappingService.ConvertToAllStoresMerchantModel(merchantStoresBundles, premiumMerchants);

            var result = new PagedList<AllStoresMerchantModel>(
                totalCount,
                allStoresModels.Count(),
                allStoresModels.ToList());
            return result;
        }

        private async Task<IEnumerable<MerchantFullView>> GetPremiumMerchants(int premiumMembership)
        {
            string sqlQuery = $@" SELECT * FROM MaterialisedMerchantFullView WITH (NOLOCK)
                                      WHERE ClientId = {premiumMembership}";
            var SpecialOffers = await _readOnlyRepository.Query<MerchantFullView>(sqlQuery);

            return SpecialOffers;
        }

        public async Task<TimeZoneInfo> GetTimezoneInfoForMerchant(int merchantId)
        {
            var getByIdQuery = @"SELECT tz.TimeZoneInfoId FROM Merchant m
                            JOIN Network n ON m.NetworkId = n.NetworkId
                            JOIN TimeZone tz ON n.TimeZoneId = tz.TimeZoneId
                            WHERE m.MerchantId = @MerchantId";

            var timeZoneInfoId = await _readOnlyRepository.QueryFirstOrDefault<string>(getByIdQuery,
                new { MerchantId = merchantId });
            return timeZoneInfoId != null
                ? TZConvert.GetTimeZoneInfo(timeZoneInfoId)
                : Constants.SydneyTimezone;
        }

        public async Task<MerchantTier> GetMerchantTierById(int merchantTierId, SqlConnection conn,
            SqlTransaction transaction)
        {
            var getByIdQuery = @"SELECT * FROM MerchantTier
                               WHERE MerchantTierId = @MerchantTierId;";
            string getTierClientsById = @"SELECT ClientId FROM MerchantTierClient
                               WHERE MerchantTierId = @MerchantTierId AND Status = 1;";
            MerchantTier merchantTier = null;
            List<int> clientIds = new List<int>();
            using (var multi = conn.QueryMultipleAsync(getByIdQuery + getTierClientsById, new { MerchantTierId = merchantTierId }, transaction).Result)
            {
                merchantTier = multi.Read<MerchantTier>().FirstOrDefault();
                if (merchantTier != null)
                    merchantTier.ClientIds = multi.Read<int>().ToList();
            }

            return merchantTier;
        }

        private async Task<PagedList<MerchantBundleBasicModel>> GetMerchantBundleByStoreKey(
            MerchantRequestInfoModel merchantRequestInfoModel)
        {
            var clientIds = new List<int> { merchantRequestInfoModel.ClientId };
            if (merchantRequestInfoModel.PremiumClientId.HasValue)
            {
                clientIds.Add(merchantRequestInfoModel.PremiumClientId.Value);
            }

            var uniqueMerchantHyphenatedStrings = await GetHyphenatedStringsByStoreFilter(
                clientIds,
                merchantRequestInfoModel.CategoryId,
                merchantRequestInfoModel.InStoreFlag);

            if (merchantRequestInfoModel.PremiumClientId.HasValue)
            {
                uniqueMerchantHyphenatedStrings = MerchantHelpers.ExcludePremiumDisabledMerchants(uniqueMerchantHyphenatedStrings);
            }

            var merchantHyphenatedStringModels = uniqueMerchantHyphenatedStrings as MerchantHyphenatedStringModel[]
                                                 ?? uniqueMerchantHyphenatedStrings.ToArray();

            var hyphenatedStrings = uniqueMerchantHyphenatedStrings.Select(h => h.HyphenatedString);
            int totalCount = hyphenatedStrings.Count();
            var pagedHyphenatedStrings = hyphenatedStrings
                .Skip(merchantRequestInfoModel.Offset)
                .Take(merchantRequestInfoModel.Limit);

            var merchantFullViewModelsForAllClients = await GetMerchantsByClientIdAndUniqueMerchantHyphendatedStringsAsync(
                clientIds,
                pagedHyphenatedStrings);

            if (merchantRequestInfoModel.PremiumClientId.HasValue)
            {
                merchantFullViewModelsForAllClients = MerchantHelpers.ExcludePremiumDisabledMerchants(merchantFullViewModelsForAllClients);
            }

            if (_featureToggle.IsEnabled(Common.FeatureFlags.IS_MERCHANT_PAUSED))
            {
                merchantFullViewModelsForAllClients = MerchantHelpers.ExcludePausedMerchants(merchantFullViewModelsForAllClients);
            }

            var merchantFullViewModels = merchantFullViewModelsForAllClients;

            var merchantStoresBundles = await FormMerchantStoreBundles(
                clientIds,
                merchantRequestInfoModel.ClientId,
                merchantRequestInfoModel.PremiumClientId,
                merchantFullViewModels,
                true);

            if (merchantRequestInfoModel.InStoreFlag == MerchantInstoreFilterEnum.InStore)
            {
                var cardLinkedMerchants = await GetCardLinkedMerchantsByClientId(clientIds);
                merchantStoresBundles = ApplyInstoreOrdering(merchantStoresBundles, merchantFullViewModels, cardLinkedMerchants);
            }

            var pagedMerchantBundleData = new PagedMerchantBundleData();
            pagedMerchantBundleData.Merchants = merchantStoresBundles.AsList();
            pagedMerchantBundleData = await PopulateOnlineStoreOffers(merchantRequestInfoModel.ClientId, pagedMerchantBundleData);

            var merchantModels = _mapper.Map<IEnumerable<MerchantBundleBasicModel>>(pagedMerchantBundleData.Merchants, opts =>
            {
                opts.Items[Constants.Mapper.CustomTrackingMerchantList] = _customTrackingMerchantList;
            }).AsList();

            return new PagedList<MerchantBundleBasicModel>(
                totalCount,
                merchantModels.Count,
                merchantModels);
        }

        private async Task<IEnumerable<MerchantStoresBundle>> FormMerchantStoreBundles(
            List<int> clientIds,
            int clientId,
            int? premiumClientId,
            IEnumerable<MerchantFullView> merchantFullViewModels,
            bool useMobileSpeficMerchants)
        {
            var result = new List<MerchantStoresBundle>();

            var mechantHyphenatedGroups = merchantFullViewModels
                .GroupBy(merchant => merchant.HyphenatedString)
                .OrderBy(x => x.Key);
            foreach (var merchantGroup in mechantHyphenatedGroups)
            {
                var bundle = new MerchantStoresBundle();
                var offlineStores = new List<MerchantItem>();
                var onlineMerchants = new List<MerchantItem>();

                foreach (var merchant in merchantGroup.Where(m => m.ClientId == clientId))
                {
                    if (_networkExtension.IsInStoreNetwork(merchant.NetworkId))
                    {
                        offlineStores.Add(new MerchantItem { BaseMerchant = merchant });
                    }
                    else
                    {
                        onlineMerchants.Add(new MerchantItem { BaseMerchant = merchant });
                    }
                }

                if (premiumClientId.HasValue)
                {
                    foreach (var merchant in merchantGroup.Where(m => m.ClientId == premiumClientId))
                    {
                        var stores = _networkExtension.IsInStoreNetwork(merchant.NetworkId) ? offlineStores : onlineMerchants;
                        var existing = stores.FirstOrDefault();
                        if (existing == null)
                        {
                            stores.Add(new MerchantItem { PremiumMerchant = merchant });
                        }
                        else
                        {
                            existing.PremiumMerchant = merchant;
                        }
                    }
                }

                var onlineMerchantToMap = onlineMerchants.FirstOrDefault(x => useMobileSpeficMerchants
                    ? _networkExtension.IsInMobileSpecificNetwork(x.NetworkId)
                    : !_networkExtension.IsInMobileSpecificNetwork(x.NetworkId));

                if (onlineMerchantToMap == null)
                {
                    onlineMerchantToMap = onlineMerchants.FirstOrDefault();
                }

                bundle.OnlineStore = _mapper.Map<MerchantStore>(onlineMerchantToMap?.Merchant, opts =>
                {
                    opts.Items[Constants.Mapper.Premium] = onlineMerchantToMap?.PremiumMerchant;
                });

                bundle.OfflineStores = offlineStores
                    .Select(offlineStore => _mapper.Map<OfflineMerchantStore>(offlineStore?.Merchant, opts =>
                    {
                        opts.Items[Constants.Mapper.Premium] = offlineStore?.PremiumMerchant;
                    }))
                    .ToList();

                foreach (var offlineStore in bundle.OfflineStores)
                {
                    offlineStore.Tiers = await GetMerchantTiersAsync(clientIds, offlineStore.MerchantId);
                }
                result.Add(bundle);
            }

            return result;
        }

        private IEnumerable<MerchantStoresBundle> ApplyInstoreOrdering(
            IEnumerable<MerchantStoresBundle> merchantStoresBundles,
            IEnumerable<MerchantFullView> merchantFullViewModels,
            IEnumerable<CardLinkedMerchantView> cardLinkedMerchants)
        {
            var orderedCardLinkedMerchants = cardLinkedMerchants
                .OrderByDescending(c => c.IsHomePageFeatured)
                .ThenByDescending(c => c.IsFeatured)
                .ThenByDescending(c => c.IsPopular)
                .ThenBy(c => c.MerchantName)
                .ToList();

            var orderedMap = orderedCardLinkedMerchants
                .Select((m, index) => new { HyphenatedString = m.MerchantHyphenatedString, Order = index })
                .GroupBy(m => m.HyphenatedString)
                .ToDictionary(g => g.Key, g => g.Min(m => m.Order));

            var mechantHyphenatedGroups = merchantFullViewModels.GroupBy(merchant => merchant.HyphenatedString)
                .OrderBy(x => x.Key);
            // the order of result insertion decides the order of merchants returned
            var merchantListInOrder = mechantHyphenatedGroups.Select(m => m.Key).ToList();

            merchantStoresBundles = merchantStoresBundles
                .Select((r, index) => new { MerchantBundle = r, Index = index })
                .OrderBy(i => orderedMap[merchantListInOrder[i.Index]])
                .Select(r => r.MerchantBundle)
                .ToList();

            return merchantStoresBundles;
        }

        private async Task<PagedMerchantBundleData> PopulateOnlineStoreOffers(int clientId,
            PagedMerchantBundleData pagedMerchantBundleData)
        {
            foreach (var merchant in pagedMerchantBundleData.Merchants)
            {
                var onlineStore = merchant.OnlineStore;
                if (onlineStore != null)
                {
                    // populate online store offers
                    onlineStore.Offers =
                        (List<MerchantStore.Offer>)await GetMerchantOffersByClientIdAndMerchantId(clientId,
                            onlineStore.MerchantId);
                }
            }

            return pagedMerchantBundleData;
        }

        private async Task<List<MerchantStore.Tier>> GetMerchantTiersAsync(List<int> clientIds, int merchantId)
        {
            var merchantTiers = await GetMerchantTierViewModelsByClientIdAndMerchantId(clientIds, merchantId);
            var generalTiers = merchantTiers
                .Select(merchantTierView => _merchantMappingService.ConvertToMerchantTierDto(merchantTierView));

            foreach (var tier in generalTiers)
            {
                var tierLinks = await GetMerchantTierLinks(tier.MerchantTierId);
                if (tierLinks.Count() > 0)
                {
                    tier.TierLinks = (List<MerchantTierLink>)tierLinks;
                }

                var merchantTier = (await GetMerchantTiers(tier.MerchantTierId))
                    .ToList()
                    .FirstOrDefault();
                tier.TierReference = merchantTier?.TierReference;
            }

            var merchantStoreTiers = generalTiers.Select(merchantTierDto =>
                _merchantMappingService.ConvertToMerchantStoreTier(merchantTierDto));

            return merchantStoreTiers.ToList();
        }

        private IEnumerable<string> GetKeysForCashbackMerchants(int ClientId, IEnumerable<MerchantHyphenatedStringModel> models) =>
           models
                .GroupBy(m => m.HyphenatedString)
                .Where(item => item.Any(group => group.ClientId == ClientId && group.ClientCommission > 0))
                .Select(entry => entry.Key)
                .Distinct();

        private async Task<IEnumerable<CardLinkedMerchantView>> GetCardLinkedMerchantsByClientId(List<int> clientIds)
        {
            string SQLQuery = @"SELECT * FROM CardLinkedMerchantView merchant
                                WHERE
                                      ClientId IN @ClientId
                                      AND InStore = 1";

            return await _readOnlyRepository.QueryAsync<CardLinkedMerchantView>(SQLQuery,
                new
                {
                    ClientId = clientIds
                });
        }

        private async Task<IEnumerable<MerchantTierView>> GetMerchantTierViewModelsByClientIdAndMerchantId(List<int> clientIds,
            int merchantId)
        {
            string SQLQuery = @"SELECT * FROM MaterialisedMerchantTierView merchant
                                WHERE
                                      ClientId in @ClientId
                                      AND MerchantId = @MerchantId
                                ";

            return await _readOnlyRepository.QueryAsync<MerchantTierView>(SQLQuery,
                new
                {
                    ClientId = clientIds,
                    MerchantId = merchantId
                });
        }

        private async Task<IEnumerable<MerchantTierLink>> GetMerchantTierLinks(int merchantTierId)
        {
            string SQLQuery = @"SELECT * FROM MerchantTierLink
                                WHERE MerchantTierId = @MerchantTierId";
            return await _readOnlyRepository.QueryAsync<MerchantTierLink>(SQLQuery, new { MerchantTierId = merchantTierId });
        }

        private async Task<IEnumerable<MerchantTier>> GetMerchantTiers(int merchantTierId)
        {
            string SQLQuery = @"SELECT * FROM MerchantTier
                               WHERE MerchantTierId = @MerchantTierId";
            return await _readOnlyRepository.QueryAsync<MerchantTier>(SQLQuery, new { MerchantTierId = merchantTierId });
        }

        private async Task<IEnumerable<MerchantHyphenatedStringModel>> GetUniqueHyphenatedStringsAsync(List<int> clientIds, int CategoryId = 0)
        {
            if (CategoryId > 0)
            {
                return await GetUniqueHyphenatedStringsByCategoryIdAsync(clientIds, CategoryId);
            }

            return await GetUniqueHyphenatedStringsForClientAsync(clientIds);
        }

        private async Task<IEnumerable<MerchantHyphenatedStringModel>> GetUniqueHyphenatedStringsByCategoryIdAsync(List<int> clientIds, int categoryId)
        {
            string SQLQuery = @"
                SELECT DISTINCT HyphenatedString, merchant.IsPremiumDisabled FROM MaterialisedMerchantFullView merchant
                    INNER JOIN MerchantCategoryMap mcm on merchant.MerchantId = mcm.MerchantId
                    WHERE
                            CategoryId = @CategoryId
                            AND ClientId IN @ClientIds
                            AND ISNULL(merchant.IsMobileAppEnabled,1) = 1
                            AND ROUND(merchant.Commission * merchant.ClientComm * merchant.MemberComm / 100.0 / 100.0, 2) > 0";

            var hyphenatedStringModels = await _readOnlyRepository.QueryAsync<MerchantHyphenatedStringModel>(SQLQuery,
                new
                {
                    CategoryId = categoryId,
                    ClientIds = clientIds
                });

            return hyphenatedStringModels.ToList();
        }

        private async Task<IEnumerable<MerchantHyphenatedStringModel>> GetUniqueHyphenatedStringsForClientAsync(List<int> clientIds)
        {
            string SQLQuery = @"
                SELECT DISTINCT HyphenatedString HyphenatedString, merchant.IsPremiumDisabled FROM MaterialisedMerchantFullView merchant
                    WHERE ISNULL(merchant.IsMobileAppEnabled,1) = 1
                        AND ClientId IN @ClientIds
                        AND ROUND(merchant.Commission * merchant.ClientComm * merchant.MemberComm / 100.0 / 100.0, 2) > 0";

            var hyphenatedStringModels = await _readOnlyRepository.QueryAsync<MerchantHyphenatedStringModel>(SQLQuery,
                new
                {
                    ClientIds = clientIds
                });

            return hyphenatedStringModels.ToList();
        }

        private async Task<IEnumerable<MerchantHyphenatedStringModel>> GetOrderedUniqueHyphenatedStringsAsync(
            int clientId, int offset, int limit, int categoryId = 0, MerchantOrderEnum order = MerchantOrderEnum.Any)
        {
            string SQLQuery = @"SELECT HyphenatedString, merchant.IsPremiumDisabled, merchant.IsPaused, 
                                ClientId, ROUND(merchant.Commission * merchant.ClientComm * merchant.MemberComm / 100.0 / 100.0, 2) ClientCommission
                                FROM MaterialisedMerchantFullView merchant
                                LEFT JOIN MerchantCategoryMap mcm on merchant.MerchantId = mcm.MerchantId
                                WHERE ClientId = @ClientId";

            if (categoryId != 0)
            {
                SQLQuery += " AND CategoryId = @CategoryId";
            }

            SQLQuery += " GROUP BY HyphenatedString, IsPremiumDisabled, IsPaused," +
                        "ClientId, ROUND(merchant.Commission * merchant.ClientComm * merchant.MemberComm / 100.0 / 100.0, 2)";

            switch (order)
            {
                case MerchantOrderEnum.Alphabetical:
                    SQLQuery += " ORDER BY HyphenatedString";
                    break;

                case MerchantOrderEnum.AlphabeticalNumbersLast:
                    SQLQuery += " ORDER BY (CASE WHEN HyphenatedString like '[a-z]%' THEN 0 ELSE 1 END), HyphenatedString;";
                    break;
            }

            var hyphenatedStringModels = await _readOnlyRepository.QueryAsync<MerchantHyphenatedStringModel>(SQLQuery,
                new
                {
                    CategoryId = categoryId,
                    ClientId = clientId
                });

            return hyphenatedStringModels.ToList();
        }

        private async Task<IEnumerable<MerchantHyphenatedStringModel>> GetHyphenatedStringsByStoreFilter(
            List<int> clientIds,
            int categoryId,
            MerchantInstoreFilterEnum instoreFilter)
        {
            var uniqueHyphenatedStrings = await GetUniqueHyphenatedStringsAsync(clientIds, categoryId);

            if (instoreFilter == MerchantInstoreFilterEnum.InStore)
            {
                return (await GethyphenatedStringsForInstore(clientIds, uniqueHyphenatedStrings));
            }

            if (instoreFilter == MerchantInstoreFilterEnum.Online)
            {
                return (await GetHyphenatedStringsForOnline(clientIds, uniqueHyphenatedStrings));
            }

            return uniqueHyphenatedStrings.OrderBy(m => m.HyphenatedString);
        }

        private async Task<MerchantCountModel> GetTotalMerchantCountForClientFromDbAsync(int clientId)
        {
            string SQLQuery = @"SELECT COUNT(DISTINCT HyphenatedString) as TotalMerchantsCount FROM MaterialisedMerchantFullView WHERE ClientId = @ClientId";

            return (await _readOnlyRepository.QueryAsync<MerchantCountModel>(SQLQuery, new { ClientId = clientId })).FirstOrDefault();
        }

        private async Task<IEnumerable<MerchantHyphenatedStringModel>> GethyphenatedStringsForInstore(
            List<int> clientIds,
            IEnumerable<MerchantHyphenatedStringModel> merchantHyphenatedStrings)
        {
            string SQLQuery =
                @"SELECT Distinct MerchantHyphenatedString HyphenatedString, IsHomePageFeatured, IsFeatured, IsPopular, MerchantName, IsPremiumDisabled
                                FROM CardLinkedMerchantView
                                WHERE ClientId IN @ClientIds
                                     AND InStore = 1
                                     AND MerchantHyphenatedString IN @UniqueMerchanthyphenatedStrings
                                ORDER BY IsHomePageFeatured desc,
                                         IsFeatured desc,
                                         IsPopular desc,
                                         MerchantName
                                ";

            var hyphenatedStringModels = await _readOnlyRepository.QueryAsync<MerchantHyphenatedStringModel>(SQLQuery
                , new
                {
                    UniqueMerchanthyphenatedStrings =
                        merchantHyphenatedStrings.Select(m => m.HyphenatedString).ToArray(),
                    ClientIds = clientIds
                });

            return hyphenatedStringModels
                .ToList();
        }

        private async Task<IEnumerable<MerchantHyphenatedStringModel>> GetHyphenatedStringsForOnline(
            List<int> clientIds,
            IEnumerable<MerchantHyphenatedStringModel> merchantHyphenatedStrings)
        {
            string SQLQuery =
                @"SELECT  MerchantHyphenatedString HyphenatedString, IsHomePageFeatured, IsFeatured, IsPopular, MerchantName, IsPremiumDisabled
                                FROM CardLinkedMerchantView
                                WHERE
                                     ClientId IN @ClientIds
                                     AND InStore = 1
                                ORDER BY hyphenatedString";

            var cardLinkedHyphenatedStringModels = await _readOnlyRepository.QueryAsync<MerchantHyphenatedStringModel>(SQLQuery,
                new
                {
                    ClientIds = clientIds
                });

            var CardLinkedHyphenatedStrings = cardLinkedHyphenatedStringModels.Select(m => m.HyphenatedString).ToList();
            return merchantHyphenatedStrings
                .Where(m => !CardLinkedHyphenatedStrings.Contains(m.HyphenatedString))
                .ToList();
        }

        private async Task<IEnumerable<MerchantFullView>> GetMerchantsByClientIdAndUniqueMerchantHyphendatedStringsAsync(
            List<int> clientIds,
            IEnumerable<string> uniqueMerchanthyphenatedStrings)
        {
            const string SQLQuery = @"SELECT * FROM MaterialisedMerchantFullView
                                      WHERE ClientId IN @ClientIds AND
                                            HyphenatedString IN @UniqueMerchanthyphenatedStrings";

                return await _readOnlyRepository.QueryAsync<MerchantFullView>(SQLQuery,
                new
                {
                    ClientIds = clientIds,
                    UniqueMerchanthyphenatedStrings = uniqueMerchanthyphenatedStrings.ToArray()
                });
        }

        private async Task<IEnumerable<MerchantStore.Offer>> GetMerchantOffersByClientIdAndMerchantId(int clientId,
            int merchantId)
        {
            string SQLQuery = @"SELECT * FROM MaterialisedOfferView
                                WHERE ClientId = @ClientId AND MerchantId = @MerchantId";

            var offerViews = await _readOnlyRepository.QueryAsync<OfferViewModel>(SQLQuery,
                new
                {
                    ClientId = clientId,
                    MerchantId = merchantId
                });

            offerViews = offerViews.OrderByDescending(offer => offer.IsFeatured)
                .ThenByDescending(offer => offer.Ranking)
                .ThenBy(offer => offer.DateEnd);

            var offers = offerViews.Select(offer => _merchantMappingService.ConvertToOffer(offer)).ToList();

            return offers;
        }

        private async Task<IEnumerable<MerchantSearchResponseModel>> GetMerchantsSearchResponseByFilter(
            MerchantByFilterRequestModel filter)
        {
            var idsWhereIn = filter.Ids.Any() ? "AND mfv.MerchantId IN @ids" : "";
            var (likeQuery, args) = GenerateLikeQuery(filter.Name);

            string sqlQuery =
                $@"SELECT mfv.MerchantId, mfv.MerchantName, mfv.NetworkId, n.NetworkName FROM MaterialisedMerchantFullView mfv
                                      INNER JOIN Network n ON mfv.NetworkId = n.NetworkId
                                      WHERE mfv.ClientId = @ClientId
                                      {likeQuery}
                                      {idsWhereIn}
                                      ORDER BY mfv.MerchantName";

            args.Add("ClientId", filter.ClientId);
            args.Add("Ids", filter.Ids);

            return await _readOnlyRepository.QueryAsync<MerchantSearchResponseModel>(sqlQuery, args);
        }

        private async Task<IEnumerable<CmsTrackingMerchantSearchResonseModel>> GetCmsTrackingMerchantsSearchResponseByFilter(
            CmsTrackingMerchantSearchFilterRequestModel filter)
        {
            var networkQuery = filter.NetworkId.HasValue ? " WHERE mer.NetworkId = @NetworkId" : "";
            string sqlQuery =
                $@"SELECT mer.MerchantId, mer.MerchantName, mer.NetworkId, mer.Status
                   FROM Merchant mer
                   {networkQuery}
                   ORDER BY mer.MerchantName";

            var args = new DynamicParameters();
            if (filter.NetworkId.HasValue)
                args.Add("NetworkId", filter.NetworkId.Value);

            var merchants = await _readOnlyRepository.QueryAsync<CmsTrackingMerchantSearchResonseModel>(sqlQuery,args);

            if (filter.Name != string.Empty)
                merchants = merchants.Where(m => m.MerchantName.StartsWith(filter.Name, StringComparison.OrdinalIgnoreCase));

            return merchants;
        }

        private Tuple<string, DynamicParameters> GenerateLikeQuery(string name)
        {
            var args = new DynamicParameters();
            if (name == String.Empty)
            {
                return new Tuple<string, DynamicParameters>(string.Empty, args);
            }

            var words = name.Split(" ");

            if (words.Length == 1)
            {
                args.Add("MerchantName", $"%{name}%");
                return new Tuple<string, DynamicParameters>("AND mfv.HyphenatedString LIKE @MerchantName", args);
            }

            var query = string.Join("AND", words.Select((x, i) =>
            {
                args.Add($"MerchantName{i}", $"%{x}%");
                return $" mfv.HyphenatedString LIKE @MerchantName{i} ";
            }).ToList());
            return new Tuple<string, DynamicParameters>($"AND {query}", args);
        }

        private async Task<MerchantSearchResponseModel> GetMerchantsSearchResponseByClientIdAndMerchantIdAsync(
            int clientId, int id)
        {
            const string SQLQuery = @"SELECT MerchantId, MerchantName FROM MaterialisedMerchantFullView
                                      WHERE ClientId = @ClientId AND MerchantId = @MerchantId ";

            return await _readOnlyRepository.QueryFirstOrDefault<MerchantSearchResponseModel>(SQLQuery
                , new
                {
                    ClientId = clientId,
                    MerchantId = id
                });
        }

        public async Task<IEnumerable<MerchantViewModel>> GetMerchantsForStandardClient(int clientId, IEnumerable<int> merchantIds)
        {
            var trendingStoreMerchants = (await GetMerchantsByClientIdAndMerchantIdsAsync(new List<int> { clientId }, merchantIds)).ToList();
            return trendingStoreMerchants;
        }

        public async Task<IEnumerable<MerchantViewModel>> GetMerchantsForStandardAndPremiumClients(int clientId, int premiumClientId, IEnumerable<int> merchantIds)
        {
            var clientIds = new List<int> { clientId, premiumClientId };

            var trendingStoreMerchants = (await GetMerchantsByClientIdAndMerchantIdsAsync(clientIds, merchantIds)).ToList();

            var standardStores = trendingStoreMerchants.Where(merchant => merchant.ClientId == clientId);
            var premiumStores = trendingStoreMerchants.Where(merchant => merchant.ClientId == premiumClientId);
            var trendingStores = SetStandardAndPremiumStoresInOrder(standardStores, premiumStores);

            trendingStores = MerchantHelpers.ExcludePremiumDisabledMerchants(trendingStores);

            return trendingStores;
        }

        private IEnumerable<MerchantViewModel> SetStandardAndPremiumStoresInOrder(
            IEnumerable<MerchantViewModel> standardMerchants, IEnumerable<MerchantViewModel> premiumMerchants)
        {
            var merahants = standardMerchants.ToDictionary(
                merchant => merchant.MerchantId, merachant => merachant);
            foreach (var premiumMerchant in premiumMerchants)
            {
                if (merahants.TryGetValue(premiumMerchant.MerchantId, out var merchant))
                {
                    merchant.Premium = _mapper.Map<PremiumMerchant>(premiumMerchant);
                }
                else
                {
                    premiumMerchant.Premium = _mapper.Map<PremiumMerchant>(premiumMerchant);
                    merahants.Add(premiumMerchant.MerchantId, premiumMerchant);
                }
            }
            return merahants.Values;
        }

        private async Task<IEnumerable<MerchantViewModel>> GetMerchantsByClientIdAndMerchantIdsAsync(
            IList<int> clientIds, IEnumerable<int> merchantIds, bool isMobileApp = false)
        {
            const string sqlQuery = @"SELECT * FROM MaterialisedMerchantView
                                      WHERE ClientId in @ClientIds";

            var merchants = await _readOnlyRepository.QueryAsync<MerchantViewModel>(sqlQuery,
                new
                {
                    ClientIds = clientIds
                });

            return merchants.Where(
                    merchant => merchantIds.Contains(merchant.MerchantId) &&
                                (!isMobileApp ||
                                 ((!merchant.IsMobileAppEnabled.HasValue ||
                                   merchant.IsMobileAppEnabled == true))))
                .OrderBy(merchant => merchant.MerchantName)
                .ToList();
        }

        public async Task<IEnumerable<MerchantViewModel>> GetPremiumDisabledMerchants()
        {
            const string sqlQuery = @"SELECT MerchantId FROM Merchant WHERE IsPremiumDisabled = 1";

            return await _readOnlyRepository.QueryAsync<MerchantViewModel>(sqlQuery);
        }
    }
}