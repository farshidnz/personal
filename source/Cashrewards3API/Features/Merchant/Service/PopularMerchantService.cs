using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Merchant.Repository;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant
{
    public interface IPopularMerchantService
    {
        Task<PagedList<MerchantDto>> GetPopularMerchantsForBrowserAsync(int clientId, int? premiumClientId, int offset = 0, int limit = 12);

        Task<PagedList<MerchantDto>> GetPopularMerchantsForMobileAsync(int clientId, int? premiumClientId, int offset = 0, int limit = 12);
    }

    public class PopularMerchantService : IPopularMerchantService
    {
        private readonly IMerchantMappingService _merchantMappingService;
        private readonly IAmazonDynamoDB _dynamoDbClient;
        private readonly IRedisUtil _redisUtil;
        private readonly CacheConfig _cacheConfig;
        private readonly ICacheKey _cacheKey;
        private readonly INetworkExtension _networkExtension;
        private readonly IMapper _mapper;
        private readonly IPopularMerchantRepository _popularMerchantRepository;
        private readonly string _trendingStoreInfoTable;
        private readonly IFeatureToggle _featureToggle;

        public PopularMerchantService(
            IConfiguration configuration,
            IMerchantMappingService merchantMappingService,
            IAmazonDynamoDB dynamoDbClient,
            IRedisUtil redisUtil,
            ICacheKey cacheKey,
            CacheConfig cacheConfig,
            INetworkExtension networkExtension,
            IMapper mapper,
            IPopularMerchantRepository popularMerchantRepository,
            IFeatureToggle featureToggle)
        {
            _merchantMappingService = merchantMappingService;
            _dynamoDbClient = dynamoDbClient;
            _redisUtil = redisUtil;
            _cacheKey = cacheKey;
            _cacheConfig = cacheConfig;
            _networkExtension = networkExtension;
            _mapper = mapper;
            _popularMerchantRepository = popularMerchantRepository;
            _trendingStoreInfoTable = configuration["Config:PopularStoreInfoTable"];
            _featureToggle = featureToggle;
        }

        public async Task<PagedList<MerchantDto>> GetPopularMerchantsForBrowserAsync(
            int clientId, int? premiumClientId, int offset = 0, int limit = 12)
        {
            var key = _cacheKey.GetPopularMerchantsForBrowserKey(clientId, premiumClientId, offset, limit);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                                    () => GetPopularMerchantsForBrowserFromDbAsync(clientId, premiumClientId, offset, limit),
                                    _cacheConfig.MerchantDataExpiry);
        }

        private async Task<PagedList<MerchantDto>> GetPopularMerchantsForBrowserFromDbAsync(
            int clientId, int? premiumClientId, int offset = 0, int limit = 12)
        {
            var popularStoreConfig = await GetPopularStoreConfig(Constants.PopularStores.Browser);
            var popularMerchants = (await GetPopularMerchants(clientId, premiumClientId, popularStoreConfig.MerchantIds))
                                         .Where(merchant => !_networkExtension.IsInMobileSpecificNetwork(merchant.NetworkId));

            if (_featureToggle.IsEnabled(Common.FeatureFlags.IS_MERCHANT_PAUSED))
            {
                popularMerchants = MerchantHelpers.ExcludePausedMerchants(popularMerchants);
            }
            var popularMerchantDtos = _merchantMappingService.ConvertToMerchantDto(popularMerchants);

            var merchantDtos = popularMerchantDtos as MerchantDto[] ?? popularMerchantDtos.ToArray();
            var pagedData = GetPopularMerchantsPagedData(merchantDtos, offset, limit).ToList();

            return new PagedList<MerchantDto>(merchantDtos.Count(), pagedData.Count(), pagedData);
        }

        public async Task<PagedList<MerchantDto>> GetPopularMerchantsForMobileAsync(
            int clientId, int? premiumClientId, int offset = 0, int limit = 12)
        {
            var key = _cacheKey.GetPopularMerchantsForMobileKey(clientId, premiumClientId, offset, limit);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                                    () => GetPopularMerchantsForMobileFromDbAsync(clientId, premiumClientId, offset, limit),
                                    _cacheConfig.MerchantDataExpiry);
        }

        private async Task<PagedList<MerchantDto>> GetPopularMerchantsForMobileFromDbAsync(
            int clientId, int? premiumClientId, int offset = 0, int limit = 12)
        {
            var popularStoreConfig = await GetPopularStoreConfig(Constants.PopularStores.Mobile);

            var popularMerchants = (await GetPopularMerchants(clientId, premiumClientId, popularStoreConfig.MerchantIds))
                                          .Where(merchant => merchant.IsMobileAppEnabled == true);

            if (_featureToggle.IsEnabled(Common.FeatureFlags.IS_MERCHANT_PAUSED))
            {
                popularMerchants = MerchantHelpers.ExcludePausedMerchants(popularMerchants);
            }

            var popularMerchantDtos = _merchantMappingService.ConvertToMerchantDto(popularMerchants);

            var merchantDtos = popularMerchantDtos as MerchantDto[] ?? popularMerchantDtos.ToArray();
            var pagedData = GetPopularMerchantsPagedData(merchantDtos, offset, limit).ToList();
            return new PagedList<MerchantDto>(merchantDtos.Count(), pagedData.Count, pagedData);
        }

        private IEnumerable<MerchantDto> GetPopularMerchantsPagedData(
            IEnumerable<MerchantDto> popularMerchantDtos, int offset = 0, int limit = 12)
        {
            if (offset > 0)
            {
                popularMerchantDtos = popularMerchantDtos.Skip(offset).ToList();
            }

            if (limit > 0)
            {
                popularMerchantDtos = popularMerchantDtos.Take(limit).ToList();
            }

            return popularMerchantDtos.ToList();
        }

        private async Task<IEnumerable<MerchantViewModel>> GetPopularMerchants(
           int clientId, int? premiumClientId, IEnumerable<int> merchantIds)
        {
            if (premiumClientId.HasValue)
            {
                var merchants = (await GetPremiumPopularMerchants(clientId, premiumClientId.Value, merchantIds))
                                        .Where( merchant => merchant.ClientCommission > 0);
                
                return MerchantHelpers.ExcludePremiumDisabledMerchants(merchants);
            }

            return (await GetPopularMerchantsFromDbAsync(clientId, merchantIds))
                                        .Where(merchant => merchant.ClientCommission > 0);
        }

        private async Task<IEnumerable<MerchantViewModel>> GetPremiumPopularMerchants(
            int clientId, int premiumClientId, IEnumerable<int> merchantIds)
        {
            var standardMerchants = await GetPopularMerchantsFromDbAsync(clientId, merchantIds);
            var premiumMerchants = await GetPopularMerchantsFromDbAsync(premiumClientId, merchantIds);
            var merchants = standardMerchants.ToDictionary(merchant => merchant.MerchantId, merchant => merchant);

            foreach (var premiumMerchant in premiumMerchants)
            {
                if (merchants.TryGetValue(premiumMerchant.MerchantId, out var merchant))
                {
                    merchant.Premium = _mapper.Map<PremiumMerchant>(premiumMerchant);
                }
                else
                {
                    premiumMerchant.Premium = _mapper.Map<PremiumMerchant>(premiumMerchant);
                    merchants.Add(premiumMerchant.MerchantId, premiumMerchant);
                }
            }

            return merchants.Values;
        }

        private async Task<IEnumerable<MerchantViewModel>> GetPopularMerchantsFromDbAsync(
            int clientId, IEnumerable<int> popularMerchantIds)
        {
            var merchants = await _popularMerchantRepository.GetPopularMerchantsFromDbAsync(clientId, popularMerchantIds);

            var popularMerchants = new List<MerchantViewModel>();
            foreach (var merchantId in popularMerchantIds)
            {
                var popularMerchant = merchants
                    .Where(merchant => merchant.MerchantId == merchantId)
                    .FirstOrDefault();

                if (popularMerchant != null)
                    popularMerchants.Add(popularMerchant);
            }

            return popularMerchants;
        }

        private async Task<PopularStoreConfig> GetPopularStoreConfig(string target)
        {
            var request = new GetItemRequest
            {
                TableName = _trendingStoreInfoTable,
                Key = new Dictionary<string, AttributeValue>
                    {
                        {"targetId", new AttributeValue{ S = target} }
                    }
            };

            var response = await _dynamoDbClient.GetItemAsync(request);

            return ConvertToPopularStoreConfig(response.Item);
        }

        private PopularStoreConfig ConvertToPopularStoreConfig(Dictionary<string, AttributeValue> item)
        {
            var popularStoreConfig = new PopularStoreConfig { };
            if (item.ContainsKey("orderedMerchantIds"))
            {
                var orderedMerchantIds = item["orderedMerchantIds"].L;
                if (orderedMerchantIds.Count > 0)
                {
                    popularStoreConfig = new PopularStoreConfig
                    {
                        MerchantIds = orderedMerchantIds
                                    .Select(m => int.Parse(m.N)).ToList()
                    };
                }
            }

            return popularStoreConfig;
        }
    }
}
