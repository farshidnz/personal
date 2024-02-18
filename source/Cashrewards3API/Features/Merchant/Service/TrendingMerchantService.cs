using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Merchant.Repository;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant
{
    public interface ITrendingMerchantService
    {
        Task<PagedList<MerchantDto>> GetTrendingStoresForBrowser(int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 12);

        Task<PagedList<MerchantDto>> GetTrendingStoresForMobile(int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 12);
    }

    public class TrendingMerchantService : ITrendingMerchantService
    {
        private readonly IMerchantMappingService _merchantMappingService;
        private readonly IRedisUtil _redisUtil;
        private readonly ICacheKey _cacheKey;
        private readonly CacheConfig _cacheConfig;
        private readonly INetworkExtension _networkExtension;
        private readonly IMapper _mapper;
        private readonly ITrendingMerchantRepository _trendingMerchantRepository;
        private readonly IAwsS3Service _awsS3Service;
        private readonly string _trendingStoreS3BucketKey;
        private readonly string _trendingStoreS3BucketName;

        public TrendingMerchantService(
            IConfiguration configuration,
            IMerchantMappingService merchantMappingService,
            IRedisUtil redisUtil,
            ICacheKey cacheKey,
            CacheConfig cacheConfig,
            INetworkExtension networkExtension,
            IMapper mapper,
            ITrendingMerchantRepository trendingMerchantRepository,
            IAwsS3Service awsS3Service)
        {
            _merchantMappingService = merchantMappingService;
            _redisUtil = redisUtil;
            _cacheConfig = cacheConfig;
            _cacheKey = cacheKey;
            _networkExtension = networkExtension;
            _mapper = mapper;
            _trendingMerchantRepository = trendingMerchantRepository;
            _awsS3Service = awsS3Service;
            _trendingStoreS3BucketKey = configuration["Config:TrendingStoreS3BucketKey"];
            _trendingStoreS3BucketName = configuration["Config:TrendingStoreS3BucketName"];
        }

        public async Task<PagedList<MerchantDto>> GetTrendingStoresForBrowser(
            int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 12)
        {
            var key = _cacheKey.GetTrendingStoresForBrowserKey(clientId, premiumClientId, categoryId, offset, limit);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                                    () => GetTrendingStoresForBrowserFromDbAsync(clientId, premiumClientId, categoryId, offset, limit),
                                    _cacheConfig.MerchantDataExpiry);
        }

        private async Task<PagedList<MerchantDto>> GetTrendingStoresForBrowserFromDbAsync(
            int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 12)
        {
            var trendingStores = ExcludeMobileSpecificNetworkIds(await GetTrendingStores(clientId, premiumClientId, categoryId));

            var trendingMerchantDtos = _merchantMappingService.ConvertToMerchantDto(trendingStores);

            var merchantDtos = trendingMerchantDtos as MerchantDto[] ?? trendingMerchantDtos.ToArray();
            var trendingPagedData = GetTrendingStoresPagedData(merchantDtos, offset, limit).ToList();
            return new PagedList<MerchantDto>(merchantDtos.Count(), trendingPagedData.Count, trendingPagedData);
        }

        public async Task<PagedList<MerchantDto>> GetTrendingStoresForMobile(
            int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 12)
        {
            var key = _cacheKey.GetTrendingStoresForMobileKey(clientId, premiumClientId, categoryId, offset, limit);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                                    () => GetTrendingStoresForMobileFromDbAsync(clientId, premiumClientId, categoryId, offset, limit),
                                    _cacheConfig.MerchantDataExpiry);
        }

        private async Task<PagedList<MerchantDto>> GetTrendingStoresForMobileFromDbAsync(
            int clientId, int? premiumClientId, int categoryId, int offset = 0, int limit = 12)
        {
            var trendingStores = await GetTrendingStores(clientId, premiumClientId, categoryId);

            trendingStores = trendingStores.Where(m => m.IsMobileAppEnabled == true).ToList();
            
            var trendingMerchantDtos = _merchantMappingService.ConvertToMerchantDto(trendingStores);

            var merchantDtos = trendingMerchantDtos as MerchantDto[] ?? trendingMerchantDtos.ToArray();
            var trendingPagedData = GetTrendingStoresPagedData(merchantDtos, offset, limit).ToList();
            return new PagedList<MerchantDto>(merchantDtos.Count(), trendingPagedData.Count, trendingPagedData);
        }

        private IEnumerable<MerchantDto> GetTrendingStoresPagedData(
            IEnumerable<MerchantDto> trendingMerchantDtos, int offset = 0, int limit = 12)
        {
            if (offset > 0)
            {
                trendingMerchantDtos = trendingMerchantDtos.Skip(offset).ToList();
            }

            if (limit > 0)
            {
                trendingMerchantDtos = trendingMerchantDtos.Take(limit).ToList();
            }

            return trendingMerchantDtos.ToList();
        }

        private async Task<IEnumerable<MerchantViewModel>> GetTrendingStores(int clientId, int? premiumClientId, int categoryId)
        {
            var trendingStoreConfig = await GetTrandingStoreConfig();

            var clientIds = new List<int> { clientId };
            if (premiumClientId.HasValue)
            {
                clientIds.Add(premiumClientId.Value);
            }
            var trendingStores = (await _trendingMerchantRepository.GetMerchantsByIdListAsync(clientIds, trendingStoreConfig.MerchantIds)).ToList();
            if (premiumClientId.HasValue)
            {
                var standardStores = trendingStores.Where(merchant => merchant.ClientId == clientId);
                var premiumStores = trendingStores.Where(merchant => merchant.ClientId == premiumClientId);
                trendingStores = SetStandardAndPremiumStoresInOrder(standardStores, premiumStores).ToList();
                trendingStores = MerchantHelpers.ExcludePremiumDisabledMerchants(trendingStores).ToList();
            }

            var orderedMerchantViewModels = OrderTrendingSotres(trendingStores, trendingStoreConfig.MerchantIds)
                .Where(model => model.ClientCommission > 0) ;
            
            if (categoryId > 0)
            {
                var categoriedMerchantIds = await _trendingMerchantRepository.GetMerchantIdsByCategoryAsync(clientIds, categoryId);
                orderedMerchantViewModels = orderedMerchantViewModels
                    .Where(p => categoriedMerchantIds.Contains(p.MerchantId))
                    .Take(15)
                    .ToList();
            }

            return orderedMerchantViewModels;
        }

        private IEnumerable<MerchantViewModel> SetStandardAndPremiumStoresInOrder(
            IEnumerable<MerchantViewModel> standardMerchants, IEnumerable<MerchantViewModel> premiumMerchants)
        {
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

        private async Task<TrendingStoreConfig> GetTrandingStoreConfig()
        {
            var configString = await AccessTredingStoreConfigJson();
            return ParseTrendingStoreConfig(configString);
        }

        private async Task<string> AccessTredingStoreConfigJson()
        {
            return await _awsS3Service.ReadAmazonS3Data(_trendingStoreS3BucketKey, _trendingStoreS3BucketName);
        }

        private TrendingStoreConfig ParseTrendingStoreConfig(string configString)
        {
            return JsonConvert.DeserializeObject<TrendingStoreConfig>(configString);
        }

        private IEnumerable<MerchantViewModel> ExcludeMobileSpecificNetworkIds(IEnumerable<MerchantViewModel> merchants)
        {
            var filteredMerchants = merchants
                    .Where(
                        merchant => !_networkExtension.IsInMobileSpecificNetwork(merchant.NetworkId)
                     ).ToList();
            return filteredMerchants;
        }

        private List<MerchantViewModel> OrderTrendingSotres(
            IEnumerable<MerchantViewModel> merhants, IList<int> trendingStoreIds)
        {
            var orderedMerchantList = new List<MerchantViewModel>();

            for (var i = 0; i < trendingStoreIds.Count; i++)
            {
                var merchant = merhants.FirstOrDefault(p => p.MerchantId == trendingStoreIds[i]);
                if (merchant != null)
                {
                    orderedMerchantList.Add(merchant);
                }
            }

            return orderedMerchantList;
        }
    }
}
