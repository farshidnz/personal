using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.Promotion;
using Cashrewards3API.Features.Promotion.Model;
using Cashrewards3API.Tests.Helpers;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using static Cashrewards3API.Common.Constants;

namespace Cashrewards3API.Tests.Features.Promotion
{
    public class PromotionCacheServiceTests
    {
        private class TestState
        {
            public PromotionCacheService PromotionCacheService { get; }

            public Mock<IPromotionService> PromotionService { get; } = new();

            public TestState()
            {
                var cacheConfig = new CacheConfig()
                {
                    MerchantDataExpiry = 30
                };

                PromotionCacheService = new PromotionCacheService(
                    Mock.Of<ICacheKey>(),
                    new RedisUtilMock().Setup<PromotionDto>().Object,
                    cacheConfig,
                    PromotionService.Object);
            }
        }

        [Test]
        public async Task GetPromotion_ShouldCallPromotionService_GivenCacheMiss()
        {
            var state = new TestState();

            await state.PromotionCacheService.GetPromotion(Clients.CashRewards, null, "mothers-day");

            state.PromotionService.Verify(p => p.GetPromotionInfo(Clients.CashRewards, null, "mothers-day"), Times.Once());
        }
    }
}
