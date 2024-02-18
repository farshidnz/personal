using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Model;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Offers;
using Cashrewards3API.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
namespace Cashrewards3API.Tests.Features.Offers
{
    public class OffersTestState
    {
        public OfferService OfferService { get; }

        public Mock<IRepository> Repository { get; private set; }
        public Mock<IFeatureToggle> FeatureToggleMock { get; init; }

        public FeatureToggle _featureToggleTrueAndPremium = new FeatureToggle() { PremiumClientId = 1000034, ShowFeature = true };


        public OffersTestState(bool useMockDb = true)
        {
            var configs = new Dictionary<string, string>
                {
                    { "Config:CustomTrackingMerchantList", "1001330" }
                };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configs)
                .AddJsonFile($"{Assembly.Load("Cashrewards3API").Folder()}/appsettings.Development.json", true)
                .Build();

            var logger = new Mock<ILogger<OfferService>>();
            var cacheKey = new Mock<ICacheKey>();
            var featureToggleMock = new Mock<IFeatureToggle>();
            var redisUtil = new RedisUtilMock();
            redisUtil.Setup<SpecialOffersDto>();
            redisUtil.Setup<PagedList<OfferDto>>();
            redisUtil.Setup<IEnumerable<OfferDto>>();

            var cacheConfig = new CacheConfig();
            var networkExtension = new Mock<INetworkExtension>();
            featureToggleMock.Setup(setup => setup.DisplayFeature(FeatureNameEnum.Premium, It.IsAny<int?>())).Returns(_featureToggleTrueAndPremium);
            featureToggleMock.Setup(setup => setup.IsEnabled(It.IsAny<string>())).Returns(false);
            OfferService = new OfferService(
                configuration,
                logger.Object,
                cacheKey.Object,
                redisUtil.Object,
                cacheConfig,
                CreateRepository(useMockDb, configuration),
                networkExtension.Object,
                featureToggleMock.Object);

            FeatureToggleMock = featureToggleMock;
        }

        public void GivenMobileDisabledMerchants(params int[] merchantIds)
        {
            Repository.Setup(o => o.GetAllAsync<MobileDisabledMerchnt>(It.IsAny<IDbTransaction>(), It.IsAny<int?>()))
                .ReturnsAsync(merchantIds.Select(id => new MobileDisabledMerchnt
                {
                    ClientId = Constants.Clients.CashRewards,
                    MerchantId = id,
                    IsMobileAppEnabled = false
                })
            );
        }

        private IRepository CreateRepository(bool useMockDb, IConfiguration configuration)
        {
            if (!useMockDb)
            {
                var shopGoDBContext = new ShopgoDBContext(new DbConfig { ShopgoDbContext = configuration["ConnectionStrings:ShopgoDbContext"] }, configuration);
                return new Repository(shopGoDBContext);
            }

            Repository = new Mock<IRepository>();
            Repository.Setup(o => o.Query<OfferViewModel>(It.Is<string>(sql => sql.Contains("IsFeatured = 1")), It.IsAny<object>(), It.IsAny<int?>()))
                .ReturnsAsync(() => OffersTestData.Where(o => o.IsFeatured).ToList());
            Repository.Setup(o => o.Query<OfferViewModel>(It.Is<string>(sql => sql.Contains("IsCashbackIncreased = 1")), It.IsAny<object>(), It.IsAny<int?>()))
                .ReturnsAsync(() => OffersTestData.Where(o => o.IsCashbackIncreased).ToList());
            Repository.Setup(o => o.Query<OfferViewModel>(It.Is<string>(sql => sql.Contains("IsPremiumFeature = 1")), It.IsAny<object>(), It.IsAny<int?>()))
                .ReturnsAsync(() => OffersTestData.Where(o => o.IsPremiumFeature).ToList());
            Repository.Setup(o => o.Query<OfferViewModel>(It.Is<string>(sql => sql.Contains("IsPremiumFeature = 1") && sql.Contains("IsCashbackIncreased = 1")), It.IsAny<object>(), It.IsAny<int?>()))
                .ReturnsAsync(() => OffersTestData.Where(o => o.IsPremiumFeature || o.IsCashbackIncreased).ToList());

            Repository.Setup(o => o.Query<OfferMerchantModel>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>()))
              .ReturnsAsync(() => MerchantTestData.ToList());
            return Repository.Object;
        }

        public List<OfferViewModel> OffersTestData { get; } = new List<OfferViewModel>
            {
                new OfferViewModel
                {
                    OfferId = 1,
                    ClientId = Constants.Clients.CashRewards,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 5m,
                    IsFeatured = true,
                    IsCashbackIncreased = true
                },
                new OfferViewModel
                {
                    OfferId = 2,
                    ClientId = Constants.Clients.CashRewards,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 6m,
                    IsFeatured = false,
                    OfferBadgeCode = Constants.BadgeCodes.TripleCashback
                },
                new OfferViewModel
                {
                    OfferId = 3,
                    ClientId = Constants.Clients.MoneyMe,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 12.25m,
                    IsFeatured = true,
                },
                new OfferViewModel
                {
                    OfferId = 4,
                    ClientId = Constants.Clients.MoneyMe,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 2m,
                    IsFeatured = false,
                },
                new OfferViewModel
                {
                    OfferId = 5,
                    ClientId = Constants.Clients.MoneyMe,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 7m,
                    IsFeatured = true,
                },
                new OfferViewModel
                {
                    OfferId = 6,
                    ClientId = Constants.Clients.Blue,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 1.25m,
                    IsFeatured = true,
                    OfferBadgeCode = Constants.BadgeCodes.AnzPremiumOffers,
                    IsPremiumFeature = true,
                    MerchantId=6
                },
                new OfferViewModel
                {
                    OfferId = 7,
                    ClientId = Constants.Clients.Blue,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 7.5m,
                    IsFeatured = true,
                    OfferBadgeCode = Constants.BadgeCodes.AnzPremiumOffers,
                    MerchantId=7
                },
                new OfferViewModel
                {
                    OfferId = 8,
                    ClientId = Constants.Clients.Blue,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 5m,
                    IsFeatured = true,
                    OfferBadgeCode = Constants.BadgeCodes.AnzPremiumOffers,
                    MerchantId=8
                }
            };

        public List<OfferViewModel> OffersSuppressedMerchantTestData { get; } = new List<OfferViewModel>
            {
                new OfferViewModel
                {
                    OfferId = 9,
                    ClientId = Constants.Clients.CashRewards,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 5m,
                    IsFeatured = true,
                    IsCashbackIncreased = true,
                    MerchantId = 6,
                    IsPremiumFeature = true,
                    MerchantIsPremiumDisabled=true
                },
                new OfferViewModel
                {
                    OfferId = 10,
                    ClientId = Constants.Clients.Blue,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 6m,
                    IsFeatured = true,
                    OfferBadgeCode = Constants.BadgeCodes.TripleCashback,
                    MerchantId = 7,
                    IsPremiumFeature = true,
                    MerchantIsPremiumDisabled=true
                },
                new OfferViewModel
                {
                    OfferId = 11,
                    ClientId = Constants.Clients.Blue,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 6m,
                    IsFeatured = true,
                    OfferBadgeCode = Constants.BadgeCodes.TripleCashback,
                    MerchantId = 8,
                    IsPremiumFeature = true,
                    MerchantIsPremiumDisabled=false
                }
            };

        public List<OfferMerchantModel> MerchantTestData { get; } = new List<OfferMerchantModel>
            {
                new OfferMerchantModel
                {
                    MerchantId = 6,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 1.25m
                },
                  new OfferMerchantModel
                {
                    MerchantId = 7,
                    Commission = 100,
                    MemberComm = 100,
                    ClientComm = 7.5m,
                }
            };
    }
}
