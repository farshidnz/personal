using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.Promotion.Model;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Promotion
{
    public interface IPromotionCacheService
    {
        Task<PromotionDto> GetPromotion(int clientId, int? premiumClientId, string slug);
    }

    public class PromotionCacheService : IPromotionCacheService
    {
        private readonly ICacheKey _cacheKey;
        private readonly IRedisUtil _redisUtil;
        private readonly CacheConfig _cacheConfig;
        private readonly IPromotionService _promotionService;

        public PromotionCacheService(
            ICacheKey cacheKey,
            IRedisUtil redisUtil,
            CacheConfig cacheConfig,
            IPromotionService promotionService)
        {
            _cacheKey = cacheKey;
            _redisUtil = redisUtil;
            _cacheConfig = cacheConfig;
            _promotionService = promotionService;
        }

        public async Task<PromotionDto> GetPromotion(int clientId, int? premiumClientId, string slug)
        {
            string key = _cacheKey.GetPromotionKey(clientId, premiumClientId, slug);
            return await _redisUtil.GetDataAsyncWithEarlyRefresh(key,
                () => _promotionService.GetPromotionInfo(clientId, premiumClientId, slug),
                _cacheConfig.MerchantDataExpiry);
        }
    }
}
