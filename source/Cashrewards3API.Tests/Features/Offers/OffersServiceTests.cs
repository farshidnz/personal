using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Model;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Offers;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Offers
{
    public class OffersServiceTests
    {
        private class TestState
        {
            public OfferService OfferService { get; }

            public Mock<IRepository> Repository { get; private set; }
            public Mock<IFeatureToggle> FeatureToggleMock { get; init; }

            public FeatureToggle _featureToggleTrueAndPremium = new FeatureToggle() { PremiumClientId = 1000034, ShowFeature = true };


            public TestState(bool useMockDb = true)
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

        #region GetFeaturedOffersForBrowserAsyncTests

        [Test]
        public async Task GetFeaturedOffersForBrowserAsync_ShouldReturnFeaturedCashrewardsOffers_GivenVariousOffers()
        {
            var state = new TestState();

            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards);
            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.CashRewards, null, 0);
            result.Data.ToList().ForEach(o => o.OfferBackgroundImageUrl = null);

            var expectedOffers = new List<OfferDto>
            {
                new OfferDto
                {
                    Id = 1,
                    IsFeatured = true,
                    OfferBadge = "",
                    ClientCommissionString = "Up to 5%",
                    IsCashbackIncreased = true,
                    Merchant = new MerchantBasicModel
                    {
                        Commission = 5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                }
            };

            result.Should().BeEquivalentTo(new PagedList<OfferDto>(expectedOffers.Count, expectedOffers.Count, expectedOffers));
        }

        [Test]
        public async Task GetFeaturedOffersAsync_ShouldReturnOfferWithoutBadgeCode_GivenANZPremiumOffersBadgeCode()
        {
            var state = new TestState();
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = 11,
                ClientId = Constants.Clients.CashRewards,
                Commission = 100,
                MemberComm = 100,
                ClientComm = 5m,
                IsFeatured = true,
                OfferBadgeCode = "ANZPremiumOffers",
                IsCashbackIncreased = true
            });
            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards);
            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.CashRewards, null, 0);
            result.Data.ToList().ForEach(o => o.OfferBackgroundImageUrl = null);

            var expectedOffers = new List<OfferDto>
            {
                new OfferDto
                {
                    Id = 1,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 5%",
                    IsCashbackIncreased = true,
                    Merchant = new MerchantBasicModel
                    {
                        Commission = 5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                },
                new OfferDto
                {
                    Id = 11,
                    IsFeatured = true,
                    OfferBadge = "",
                    ClientCommissionString = "Up to 5%",
                    IsCashbackIncreased = true,
                    Merchant = new MerchantBasicModel
                    {
                        Commission = 5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                }
            };
            var test = new PagedList<OfferDto>(expectedOffers.Count, expectedOffers.Count, expectedOffers);
            result.Should().BeEquivalentTo(test);

        }

        [Test]
        public async Task GetFeaturedOffersForBrowserAsync_ShouldReturnFeaturedMoneyMeOffers_GivenVariousOffers()
        {
            var state = new TestState();
            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.MoneyMe);

            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.MoneyMe, null, 0);
            result.Data.ToList().ForEach(o => o.OfferBackgroundImageUrl = null);

            var expectedOffers = new List<OfferDto>
            {
                new OfferDto
                {
                    Id = 3,
                    IsFeatured = true,
                    ClientCommissionString = "Up to 12.25%",
                    OfferBadge = string.Empty,
                    Merchant = new MerchantBasicModel
                    {
                        Commission = 12.25m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback",
                    },
                },
                new OfferDto
                {
                    Id = 5,
                    IsFeatured = true,
                    ClientCommissionString = "Up to 7%",
                    OfferBadge = string.Empty,
                    Merchant = new MerchantBasicModel
                    {
                        Commission = 7m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                }
            };
            result.Should().BeEquivalentTo(new PagedList<OfferDto>(expectedOffers.Count, expectedOffers.Count, expectedOffers));
        }

        [Test]
        public async Task GetFeaturedOffersForBrowserAsync_ShouldReturnStandardAndPremiumFeaturedOffers_GivenPremiumOffers()
        {
            var state = new TestState();
            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);

            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);
            result.Data.ToList().ForEach(o => o.OfferBackgroundImageUrl = null);

            var expectedOffers = new List<OfferDto>
            {
                new OfferDto
                {
                    Id = 1,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 5%",
                    IsCashbackIncreased = true,
                    Merchant = new MerchantBasicModel
                    {
                        Commission = 5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                },
                new OfferDto
                {
                    Id = 6,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 1.25%",
                    IsPremiumFeature = true,
                    MerchantId = 6,
                    Merchant = new MerchantBasicModel
                    {
                        Id = 6,
                        Commission = 1.25m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                    Premium = new Premium
                    {
                        Commission = 1.25m,
                        IsFlatRate = false,
                        ClientCommissionString = "Up to 1.25%"
                    },
                },
                new OfferDto
                {
                    Id = 7,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 7.5%",
                    MerchantId = 7,
                    Merchant = new MerchantBasicModel
                    {
                        Id =7,
                        Commission = 7.5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                    Premium = new Premium
                    {
                        Commission = 7.5m,
                        IsFlatRate = false,
                        ClientCommissionString = "Up to 7.5%"
                    },
                },
                new OfferDto
                {
                    Id = 8,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 5%",
                    MerchantId = 8,
                    Merchant = new MerchantBasicModel
                    {
                        Id =8,
                        Commission = 5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                    Premium = new Premium
                    {
                        Commission = 5m,
                        IsFlatRate = false,
                        ClientCommissionString = "Up to 5%"
                    },
                }
            };
            result.Should().BeEquivalentTo(new PagedList<OfferDto>(expectedOffers.Count, expectedOffers.Count, expectedOffers));
        }

        [Test]
        public async Task GetFeaturedOffersForBrowserAsync_ShouldReturnConjoinedStandardAndPremiumFeaturedOffers_GivenPremiumOffersWithTheSameOfferId()
        {
            var state = new TestState();
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = 1,
                Commission = 100,
                MemberComm = 100,
                ClientComm = 7m,
                ClientId = Constants.Clients.Blue,
                IsFeatured = true,
                OfferBadgeCode = "ANZPremiumOffers"
            });

            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);
            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);
            result.Data.ToList().ForEach(o => o.OfferBackgroundImageUrl = null);

            var expectedOffers = new List<OfferDto>
            {
                new OfferDto
                {
                    Id = 1,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 5%",
                    IsCashbackIncreased = true,
                    Merchant = new MerchantBasicModel
                    {
                        Commission = 5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                    Premium = new Premium
                    {
                        Commission = 7m,
                        IsFlatRate = false,
                        ClientCommissionString = "Up to 7%"
                    },
                },
                new OfferDto
                {
                    Id = 6,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 1.25%",
                    IsPremiumFeature = true,
                    MerchantId = 6,
                    Merchant = new MerchantBasicModel
                    {
                        Id =6,
                        Commission = 1.25m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                    Premium = new Premium
                    {
                        Commission = 1.25m,
                        IsFlatRate = false,
                        ClientCommissionString = "Up to 1.25%"
                    },
                },
                new OfferDto
                {
                    Id = 7,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 7.5%",
                    MerchantId = 7,
                    Merchant = new MerchantBasicModel
                    {
                        Id =7,
                        Commission = 7.5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                    Premium = new Premium
                    {
                        Commission = 7.5m,
                        IsFlatRate = false,
                        ClientCommissionString = "Up to 7.5%"
                    },
                },
                new OfferDto
                {
                    Id = 8,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 5%",
                    MerchantId = 8,
                    Merchant = new MerchantBasicModel
                    {
                        Id =8,
                        Commission = 5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                    Premium = new Premium
                    {
                        Commission = 5m,
                        IsFlatRate = false,
                        ClientCommissionString = "Up to 5%"
                    },
                }
            };

            result.Should().BeEquivalentTo(new PagedList<OfferDto>(expectedOffers.Count, expectedOffers.Count, expectedOffers));
        }

        [Test]
        public async Task GetFeaturedOffersForBrowserAsync_ShouldReturnFeaturedOffersMerchantHasCashback_GivenBothCashbackAndNoCashbackMerchants()
        {
            var state = new TestState();
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = 10,
                OfferTitle = "Offer with no cashback merchant",
                Commission = 0,
                MemberComm = 0,
                ClientComm = 0m,
                ClientId = Constants.Clients.CashRewards,
                IsFeatured = true,
                OfferBadgeCode = "ANZPremiumOffers"
            });

            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);
            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

            var resultedIds = result.Data.Select(offer => offer.Id);
            var noCashbackOfferIds = new List<int> { 10 };
            ;
            resultedIds.Should().NotContain(noCashbackOfferIds);

        }

        [Test]
        public async Task GetFeaturedOffersForBrowserAsync_ShouldReturnFeaturedOffers_WithSuppressedMerchantsForNonPremiumMembers()
        {
            var state = new TestState();

            state.OffersTestData.AddRange(state.OffersSuppressedMerchantTestData);

            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards);
            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.CashRewards, null, 0);

            var expectedOffers = new int[]
            {
                1,9
            };

            var resultIds = result.Data.Select(offer => offer.Id);

            resultIds.Should().BeEquivalentTo(expectedOffers);
        }

        [Test]
        public async Task GetFeaturedOffersForBrowserAsync_ShouldReturnFeaturedOffers_WithoutSuppressedMerchantsForPremiumMembers()
        {
            var state = new TestState();
            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);
            state.OffersTestData.AddRange(state.OffersSuppressedMerchantTestData);

            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

            var expectedOffers = new int[]
            {
                1,6,7,8,11
            };

            var resultIds = result.Data.Select(offer => offer.Id);

            resultIds.Should().BeEquivalentTo(expectedOffers);

        }

        #region Feature_ISMERCHANTPAUSED CPS-68

        [Test]
        public async Task GetFeaturedOffersForBrowserAsync_ShouldReturnFeaturedOffersIfMerchantsAreNotPaused_And_Feature_Flag_Is_On()
        {
            var state = new TestState();
            state.FeatureToggleMock.Setup(setup => setup.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(true);
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = 10,
                OfferTitle = "Offer with 5 % cashback merchant",
                Commission = 10,
                MemberComm = 10,
                ClientComm = 10m,
                ClientId = Constants.Clients.CashRewards,
                IsFeatured = true,
                OfferBadgeCode = Constants.BadgeCodes.TripleCashback,
                IsMerchantPaused = false
            });

            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);
            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

            var resultedIds = result.Data.Select(offer => offer.Id);
            var noCashbackOfferIds = new List<int> { 10 };

            resultedIds.Should().Contain(noCashbackOfferIds);
        }

        [Test]
        public async Task GetFeaturedOffersForBrowserAsync_ShouldNotReturnFeaturedOffersIfMerchantsArePaused_And_Feature_Flag_Is_On()
        {
            var state = new TestState();
            state.FeatureToggleMock.Setup(setup => setup.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(true);

            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = 10,
                OfferTitle = "Offer with 5 % cashback merchant",
                Commission = 10,
                MemberComm = 10,
                ClientComm = 10m,
                ClientId = Constants.Clients.CashRewards,
                IsFeatured = true,
                OfferBadgeCode = Constants.BadgeCodes.TripleCashback,
                IsMerchantPaused = true
            });

            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);
            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

            var resultedIds = result.Data.Select(offer => offer.Id);
            var noCashbackOfferIds = new List<int> { 10 };

            resultedIds.Should().NotContain(noCashbackOfferIds);
        }

        [Test]
        public async Task GetFeaturedOffersForBrowserAsync_ShouldNotReturnFeaturedOffersIfMerchantsArePaused_And_Feature_Flag_Is_On_And_ANZ_Client()
        {
            var state = new TestState();
            state.FeatureToggleMock.Setup(setup => setup.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(true);

            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = 10,
                OfferTitle = "Offer with 5 % cashback merchant",
                Commission = 10,
                MemberComm = 10,
                ClientComm = 10m,
                ClientId = Constants.Clients.Blue,
                IsFeatured = true,
                OfferBadgeCode = Constants.BadgeCodes.AnzPremiumOffers,
                IsMerchantPaused = true
            });

            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);
            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

            var resultedIds = result.Data.Select(offer => offer.Id);
            var noCashbackOfferIds = new List<int> { 10 };

            resultedIds.Should().NotContain(noCashbackOfferIds);
        }

        [Test]
        public async Task GetFeaturedOffersForBrowserAsync_ShouldReturnFeaturedOffersIfMerchantsArePaused_And_Feature_Flag_Is_Off()
        {
            var state = new TestState();
            state.FeatureToggleMock.Setup(setup => setup.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(false);

            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = 10,
                OfferTitle = "Offer with 5 % cashback merchant",
                Commission = 10,
                MemberComm = 10,
                ClientComm = 10m,
                ClientId = Constants.Clients.CashRewards,
                IsFeatured = true,
                OfferBadgeCode = Constants.BadgeCodes.TripleCashback,
                IsMerchantPaused = true
            });

            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);
            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

            var resultedIds = result.Data.Select(offer => offer.Id);
            var noCashbackOfferIds = new List<int> { 10 };

            resultedIds.Should().Contain(noCashbackOfferIds);
        }

        #endregion Feature_ISMERCHANTPAUSED CPS-68

        #endregion

        #region GetFeaturedOffersForMobileAsyncTests

        [Test]
        public async Task GetFeaturedOffersAsyncForMobile_ShouldNotReturnDesktopOffers_GivenNonMobileEnabledOffers()
        {
            var state = new TestState();
            state.Repository.Setup(o => o.Query<OfferViewModel>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>()))
                .ReturnsAsync(() => state.OffersTestData.Where(o => o.ClientId == (int)ClientsEnum.CashRewards).ToList());
            state.GivenMobileDisabledMerchants(124);
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = 20,
                ClientId = Constants.Clients.CashRewards,
                Commission = 100,
                MemberComm = 100,
                ClientComm = 25m,
                IsFeatured = true,
                IsCashbackIncreased = true,
                MerchantId = 124
            });

            var result = await state.OfferService.GetFeaturedOffersAsync(Constants.Clients.CashRewards, null, 0, 0, 35, isMobileDevice: true);

            result.Data.Should().NotContain(o => o.Id == 20);
        }

        [Test]
        public async Task GetSpecialOffersForBrowserAsync_When_MerchantsIsPausedFeatureFlagIsOff_Then_ShouldReturnMerchantPausedOffers()
        {
            var state = new TestState();
            state.FeatureToggleMock.Setup(setup => setup.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(false);

            var expectedOfferId = 10;
            var expectedOfferId2 = 11;

            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = expectedOfferId,
                OfferTitle = "Offer with 5 % cashback merchant",
                Commission = 10,
                MemberComm = 10,
                ClientComm = 10m,
                ClientId = Constants.Clients.CashRewards,
                IsFeatured = true,
                IsCashbackIncreased = true,
                OfferBadgeCode = Constants.BadgeCodes.TripleCashback,
                IsMerchantPaused = true
            });
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = expectedOfferId2,
                OfferTitle = "Offer with 5 % cashback merchant",
                Commission = 10,
                MemberComm = 10,
                ClientComm = 10m,
                ClientId = Constants.Clients.Blue,
                IsFeatured = true,
                IsPremiumFeature = true,
                OfferBadgeCode = Constants.BadgeCodes.TripleCashback,
                IsMerchantPaused = true
            });

            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, null, null, false);

            var resultedIds = result.CashBackIncreasedOffers.Select(offer => offer.Id).Union(result.PremiumFeatureOffers.Select(offer => offer.Id));
            var merchantPausedOfferIds = new List<int> { expectedOfferId, expectedOfferId2 };

            resultedIds.Should().Contain(merchantPausedOfferIds);
        }

        [Test]
        public async Task GetSpecialOffersForBrowserAsync_When_MerchantsIsPausedFeatureFlagIsOn_Then_ShouldNotReturnMerchantPausedOffers()
        {
            var state = new TestState();
            state.FeatureToggleMock.Setup(setup => setup.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(true);

            var expectedOfferId = 10;
            var expectedOfferId2 = 11;

            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = expectedOfferId,
                OfferTitle = "Offer with 5 % cashback merchant",
                Commission = 10,
                MemberComm = 10,
                ClientComm = 10m,
                ClientId = Constants.Clients.CashRewards,
                IsFeatured = true,
                IsCashbackIncreased = true,
                OfferBadgeCode = Constants.BadgeCodes.TripleCashback,
                IsMerchantPaused = true
            });
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = expectedOfferId2,
                OfferTitle = "Offer with 5 % cashback merchant",
                Commission = 10,
                MemberComm = 10,
                ClientComm = 10m,
                ClientId = Constants.Clients.Blue,
                IsFeatured = true,
                IsPremiumFeature = true,
                OfferBadgeCode = Constants.BadgeCodes.TripleCashback,
                IsMerchantPaused = true
            });

            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);
            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, null, null, false);

            var resultedIds = result.CashBackIncreasedOffers.Select(offer => offer.Id).Union(result.PremiumFeatureOffers.Select(offer => offer.Id));
            var specialOfferIds = new List<int> { expectedOfferId, expectedOfferId2 };

            resultedIds.Should().NotContain(specialOfferIds);
        }


        [Test]
        public async Task GetIncreasedOffersForMobileAsync_When_MerchantsIsPausedFeatureFlagIsOff_Then_ShouldReturnMerchantPausedOffers()
        {
            var state = new TestState();
            state.FeatureToggleMock.Setup(setup => setup.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(false);

            var expectedOfferId = 10;

            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = expectedOfferId,
                OfferTitle = "Offer with 5 % cashback merchant",
                Commission = 10,
                MemberComm = 10,
                ClientComm = 10m,
                ClientId = Constants.Clients.CashRewards,
                IsFeatured = true,
                IsCashbackIncreased = true,
                OfferBadgeCode = Constants.BadgeCodes.TripleCashback,
                IsMerchantPaused = true
            });

            var result = await state.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, null);

            var expectedOfferIds = new List<int> { expectedOfferId };

            result.Select(o => o.Id).Should().Contain(expectedOfferIds);
        }


        [Test]
        public async Task GetIncreasedOffersForMobileAsync_When_MerchantsIsPausedFeatureFlagIsOn_Then_ShouldNotReturnMerchantPausedOffers()
        {
            var state = new TestState();
            state.FeatureToggleMock.Setup(setup => setup.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(true);

            var expectedOfferId = 10;

            state.OffersTestData.RemoveAll(p => p.ClientId != Constants.Clients.CashRewards && p.ClientId != Constants.Clients.Blue);
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = expectedOfferId,
                OfferTitle = "Offer with 5 % cashback merchant",
                Commission = 10,
                MemberComm = 10,
                ClientComm = 10m,
                ClientId = Constants.Clients.CashRewards,
                IsFeatured = true,
                IsCashbackIncreased = true,
                OfferBadgeCode = Constants.BadgeCodes.TripleCashback,
                IsMerchantPaused = true
            });

            var result = await state.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, null);

            var expectedOfferIds = new List<int> { expectedOfferId };

            result.Select(o => o.Id).Should().NotContain(expectedOfferIds);
        }

        #endregion

        #region GetCashBackIncreasedOffersForMobileTests

        [Test]
        public async Task GetCashBackIncreasedOffersForMobile_ShouldReturnCashbackIncreasedOffers()
        {
            var state = new TestState();

            var result = await state.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, null);
            result.ToList().ForEach(o => o.OfferBackgroundImageUrl = null);

            result.Should().BeEquivalentTo(new List<OfferDto>
            {
                new OfferDto
                {
                    Id = 1,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 5%",
                    IsCashbackIncreased = true,
                    Merchant = new MerchantBasicModel
                    {
                        Commission = 5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                }

            });
        }

        [Test]
        public async Task GetCashBackIncreasedOffersForMobile_ShouldNotReturnMobileExcludedOffers()
        {
            var state = new TestState();
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = 20,
                ClientId = Constants.Clients.CashRewards,
                Commission = 0,
                IsFeatured = true,
                IsCashbackIncreased = true,
                MerchantId = 124
            });

            var result = await state.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, null);

            var expectedOfferIds = new List<int> { 1 };
            var resultOfferIds = result.Select(offer => offer.Id);
            resultOfferIds.Should().BeEquivalentTo(expectedOfferIds);
        }

        [Test]
        public async Task GetCashBackIncreasedOffersForMobile_ShouldNotReturnNoCashbackOffers()
        {
            var state = new TestState();
            state.GivenMobileDisabledMerchants(124);
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = 20,
                ClientId = Constants.Clients.CashRewards,
                Commission = 100,
                MemberComm = 100,
                ClientComm = 25m,
                IsFeatured = true,
                IsCashbackIncreased = true,
                MerchantId = 124
            });

            var result = await state.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, null);
            result.ToList().ForEach(o => o.OfferBackgroundImageUrl = null);

            result.Should().BeEquivalentTo(new List<OfferDto>
            {
                new OfferDto
                {
                    Id = 1,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 5%",
                    IsCashbackIncreased = true,
                    Merchant = new MerchantBasicModel
                    {
                        Commission = 5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    }
                }
            });
        }

        [Test]
        public async Task GetCashBackIncreasedOffersForMobile_ShouldReturnPremiumOffers()
        {
            var state = new TestState();
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = 20,
                ClientId = Constants.Clients.Blue,
                Commission = 100,
                MemberComm = 100,
                ClientComm = 25m,
                IsFeatured = true,
                IsCashbackIncreased = true
            });

            var result = await state.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, Constants.Clients.Blue);
            result.ToList().ForEach(o => o.OfferBackgroundImageUrl = null);

            result.Should().BeEquivalentTo(new List<OfferDto>
            {
                new OfferDto
                {
                    Id = 1,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 5%",
                    IsCashbackIncreased = true,
                    Merchant = new MerchantBasicModel
                    {
                        Commission = 5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                },
                new OfferDto
                {
                    Id = 20,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 25%",
                    IsCashbackIncreased = true,
                    Merchant = new MerchantBasicModel
                    {
                        Commission = 25m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                    Premium = new Premium
                    {
                        Commission = 25m,
                        IsFlatRate = false,
                        ClientCommissionString = "Up to 25%"
                    },
                }
            });
        }

        [Test]
        public async Task GetCashBackIncreasedOffersForMobile_ShouldReturnConjoinedStandardAndPremiumOffers_GivenPremiumOffersWithTheSameOfferId()
        {
            var state = new TestState();
            state.OffersTestData.Add(new OfferViewModel
            {
                OfferId = 1,
                Commission = 100,
                MemberComm = 100,
                ClientComm = 7m,
                ClientId = Constants.Clients.Blue,
                IsCashbackIncreased = true,
                IsFlatRate = false,
                OfferBadgeCode = "ANZPremiumOffers"
            });

            var result = await state.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, Constants.Clients.Blue);
            result.ToList().ForEach(o => o.OfferBackgroundImageUrl = null);

            result.Should().BeEquivalentTo(new List<OfferDto>
            {
                new OfferDto
                {
                    Id = 1,
                    IsFeatured = true,
                    OfferBadge = string.Empty,
                    ClientCommissionString = "Up to 5%",
                    IsCashbackIncreased = true,
                    Merchant = new MerchantBasicModel
                    {
                        Commission = 5m,
                        CommissionType = string.Empty,
                        MerchantBadge = string.Empty,
                        RewardType = "Cashback"
                    },
                    Premium = new Premium
                    {
                        Commission = 7m,
                        IsFlatRate = false,
                        ClientCommissionString = "Up to 7%"
                    },
                }
            });
        }

        [Test]
        public async Task GetCashBackIncreasedOffersForMobile_ShouldNotReturnAnzBoostedBadge()
        {
            var state = new TestState();
            state.OffersTestData.Single(o => o.OfferId == 6).IsCashbackIncreased = true;
            state.OffersTestData.Single(o => o.OfferId == 6).OfferBadgeCode = Constants.BadgeCodes.AnzPremiumOffers;

            var result = await state.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, Constants.Clients.Blue);

            result.Single(o => o.Id == 6).OfferBadge.Should().Be(string.Empty);
        }

        [Test]
        public async Task GetCashBackIncreasedOffersForMobile_ShouldReturnOtherBadges()
        {
            var state = new TestState();
            state.OffersTestData.Single(o => o.OfferId == 6).IsCashbackIncreased = true;
            state.OffersTestData.Single(o => o.OfferId == 6).OfferBadgeCode = Constants.BadgeCodes.TripleCashback;

            var result = await state.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, Constants.Clients.Blue);

            result.Single(o => o.Id == 6).OfferBadge.Should().Be(Constants.BadgeCodes.TripleCashback);
        }

        [Test]
        public async Task GetCashBackIncreasedOffersForMobile_ShouldReturnSuppressedMerchants_IfNonPremiumMember()
        {
            var state = new TestState();
            state.OffersSuppressedMerchantTestData.ForEach(offer =>
            {
                offer.IsCashbackIncreased = true;
                offer.OfferBadgeCode = Constants.BadgeCodes.TripleCashback;
            });
            state.OffersTestData.AddRange(state.OffersSuppressedMerchantTestData);

            var result = await state.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, null);

            var expectedOfferIds = new int[] { 1, 9 };
            var resultOfferIds = result.Select(offer => offer.Id);

            resultOfferIds.Should().BeEquivalentTo(expectedOfferIds);
        }

        [Test]
        public async Task GetCashBackIncreasedOffersForMobile_ShouldNotReturnSuppressedMerchants_IfPremiumMember()
        {
            var state = new TestState();
            state.OffersSuppressedMerchantTestData.ForEach(offer =>
            {
                offer.IsCashbackIncreased = true;
                offer.OfferBadgeCode = Constants.BadgeCodes.TripleCashback;
            });
            state.OffersTestData.AddRange(state.OffersSuppressedMerchantTestData);

            var result = await state.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, Constants.Clients.Blue);

            var expectedOfferIds = new int[] { 1, 11 };
            var resultOfferIds = result.Select(offer => offer.Id);

            resultOfferIds.Should().BeEquivalentTo(expectedOfferIds);
        }

        #endregion

        #region GetCashBackIncreasedOffersForBrowserTests

        [Test]
        public async Task GetCashBackIncreasedOffersForBrowser_ShouldNotReturnAnzBoostedBadge()
        {
            var state = new TestState();
            state.OffersTestData.Single(o => o.OfferId == 6).IsCashbackIncreased = true;
            state.OffersTestData.Single(o => o.OfferId == 6).OfferBadgeCode = Constants.BadgeCodes.AnzPremiumOffers;

            var result = await state.OfferService.GetCashBackIncreasedOffersForBrowser(Constants.Clients.CashRewards, Constants.Clients.Blue);

            result.Single(o => o.Id == 6).OfferBadge.Should().Be(string.Empty);
        }

        [Test]
        public async Task GetCashBackIncreasedOffersForBrowser_ShouldReturnOtherBadges()
        {
            var state = new TestState();
            state.OffersTestData.Single(o => o.OfferId == 6).IsCashbackIncreased = true;
            state.OffersTestData.Single(o => o.OfferId == 6).OfferBadgeCode = Constants.BadgeCodes.TripleCashback;

            var result = await state.OfferService.GetCashBackIncreasedOffersForMobile(Constants.Clients.CashRewards, Constants.Clients.Blue);

            result.Single(o => o.Id == 6).OfferBadge.Should().Be(Constants.BadgeCodes.TripleCashback);
        }

        [Test]
        public async Task GetCashBackIncreasedOffersForBrowser_ShouldNotReturnOfferWithNoCashbackMerchant()
        {
            var state = new TestState();
            state.OffersTestData.ForEach(offer => { offer.IsCashbackIncreased = true; });
            var offerWithNoCashBack = state.OffersTestData.Single(o => o.OfferId == 1).Commission = 0;

            var result = await state.OfferService.GetCashBackIncreasedOffersForBrowser(Constants.Clients.CashRewards, Constants.Clients.Blue);

            var expectedOfferIds = new int[] { 2, 6, 7, 8 };
            var resultOfferIds = result.Select(offer => offer.Id);
            resultOfferIds.Should().BeEquivalentTo(expectedOfferIds);
        }

        [Test]
        public async Task GetCashBackIncreasedOffersForBrowser_ShouldReturnSuppressedMerchants_IfNonPremiumClient()
        {
            var state = new TestState();
            state.OffersSuppressedMerchantTestData.ForEach(offer => offer.IsCashbackIncreased = true);
            state.OffersTestData.AddRange(state.OffersSuppressedMerchantTestData);
            //state.OffersTestData.Single(o => o.OfferId == 6).OfferBadgeCode = Constants.BadgeCodes.AnzPremiumOffers;

            var result = await state.OfferService.GetCashBackIncreasedOffersForBrowser(Constants.Clients.CashRewards, null);

            var expectedOfferIds = new int[] { 1, 9 };
            var resultOfferIds = result.Select(offer => offer.Id);
            resultOfferIds.Should().BeEquivalentTo(expectedOfferIds);
        }

        [Test]
        public async Task GetCashBackIncreasedOffersForBrowser_ShouldNotReturnSuppressedMerchants_IfPremiumClient()
        {
            var state = new TestState();
            state.OffersSuppressedMerchantTestData.ForEach(offer => offer.IsCashbackIncreased = true);
            state.OffersTestData.AddRange(state.OffersSuppressedMerchantTestData);
            //state.OffersTestData.Single(o => o.OfferId == 6).OfferBadgeCode = Constants.BadgeCodes.AnzPremiumOffers;

            var result = await state.OfferService.GetCashBackIncreasedOffersForBrowser(Constants.Clients.CashRewards, Constants.Clients.Blue);

            var expectedOfferIds = new int[] { 1, 11 };
            var resultOfferIds = result.Select(offer => offer.Id);
            resultOfferIds.Should().BeEquivalentTo(expectedOfferIds);
        }

        #endregion

        #region GetSpecialOffersTests

        [Test]
        public async Task GetSpecialOffers_ShouldReturnCashbackIncreasedAndPremiumFeature_ForCashRewards()
        {
            var state = new TestState();

            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, null, null, false);

            result.PremiumFeatureOffers.Count().Should().BeGreaterThan(0);
            result.CashBackIncreasedOffers.Count().Should().BeGreaterThan(0);
        }

        [Test]
        public async Task GetSpecialOffers_ShouldNotReturnCashbackIncreasedAndPremiumFeature_ForMerchantsWithNoCashback()
        {
            var state = new TestState();
            state.OffersTestData.Single(offer => offer.OfferId == 1).Commission = 0;
            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, null, null, false);

            result.PremiumFeatureOffers.Count().Should().Be(1);
            result.CashBackIncreasedOffers.Count().Should().Be(0);
        }

        [Test]
        public async Task GetSpecialOffers_ShouldReturnCashbackIncreased_ForCashRewardsAndOfferTypeCashBackIncrease()
        {
            var state = new TestState();

            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, null, OfferTypeEnum.CashbackIncreased, false);

            result.PremiumFeatureOffers.Should().BeNull();
            result.CashBackIncreasedOffers.Count().Should().BeGreaterThan(0);
        }

        [Test]
        public async Task GetSpecialOffers_ShouldReturnPremiumFeatured_ForCashRewardsAndPremiumFeature_GivenOnlyCRClient()
        {
            var state = new TestState();

            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, null, OfferTypeEnum.PremiumFeature, false);

            result.PremiumFeatureOffers.Count().Should().BeGreaterThan(0);
            result.CashBackIncreasedOffers.Should().BeNull();
        }

        [Test]
        public async Task GetSpecialOffers_ShouldReturnPremiumFeatured_ForCashRewardsAndPremiumFeature_GivenCRAndBlueClient()
        {
            var state = new TestState();

            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, Constants.Clients.Blue, OfferTypeEnum.PremiumFeature, false);

            result.PremiumFeatureOffers.Count().Should().BeGreaterThan(0);
            result.CashBackIncreasedOffers.Should().BeNull();
        }

        [Test]
        public async Task GetSpecialOffers_ShouldReturnAnzBoostedBadgeForPremiumFeatureOffers()
        {
            var state = new TestState();
            state.OffersTestData.Single(o => o.OfferId == 6).OfferBadgeCode = Constants.BadgeCodes.AnzPremiumOffers;

            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, null, OfferTypeEnum.PremiumFeature, false);

            result.PremiumFeatureOffers.Single(o => o.Id == 6).OfferBadge.Should().Be(Constants.BadgeCodes.AnzPremiumOffers);
        }

        [Test]
        public async Task GetSpecialOffers_ShouldNotReturnOtherBadgesForPremiumFeatureOffers()
        {
            var state = new TestState();
            state.OffersTestData.Single(o => o.OfferId == 6).OfferBadgeCode = Constants.BadgeCodes.TripleCashback;

            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, null, OfferTypeEnum.PremiumFeature, false);

            result.PremiumFeatureOffers.Single(o => o.Id == 6).OfferBadge.Should().Be(string.Empty);
        }

        [Test]
        public async Task GetSpecialOffers_ShouldNotReturnAnzBoostedBadgeForCashbackIncreasedOffers()
        {
            var state = new TestState();
            state.OffersTestData.Single(o => o.OfferId == 6).IsCashbackIncreased = true;
            state.OffersTestData.Single(o => o.OfferId == 6).OfferBadgeCode = Constants.BadgeCodes.AnzPremiumOffers;

            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, Constants.Clients.Blue, OfferTypeEnum.CashbackIncreased, false);

            result.CashBackIncreasedOffers.Single(o => o.Id == 6).OfferBadge.Should().Be(string.Empty);
        }

        [Test]
        public async Task GetSpecialOffers_ShouldReturnOtherBadgesForCashbackIncreasedOffers()
        {
            var state = new TestState();
            state.OffersTestData.Single(o => o.OfferId == 6).IsCashbackIncreased = true;
            state.OffersTestData.Single(o => o.OfferId == 6).OfferBadgeCode = Constants.BadgeCodes.TripleCashback;

            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, Constants.Clients.Blue, OfferTypeEnum.CashbackIncreased, false);

            result.CashBackIncreasedOffers.Single(o => o.Id == 6).OfferBadge.Should().Be(Constants.BadgeCodes.TripleCashback);
        }

        [Test]
        public async Task GetSpecialOffers_ShouldReturnSuppressedMerchantOffers_IfNonPremiumMember()
        {
            var state = new TestState();
            state.OffersTestData.Single(offer => offer.OfferId == 1).IsPremiumFeature = true;
            state.OffersTestData.AddRange(state.OffersSuppressedMerchantTestData);
            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, null, OfferTypeEnum.PremiumFeature, false);

            var expectedOfferIds = new int[] { 6, 10, 11 };
            var resultOfferIds = result.PremiumFeatureOffers.Select(offer => offer.Id);

            resultOfferIds.Should().BeEquivalentTo(expectedOfferIds);
        }

        [Test]
        public async Task GetSpecialOffers_ShouldNotReturnSuppressedMerchantOffers_IfPremiumMember()
        {
            var state = new TestState();
            state.OffersTestData.Single(offer => offer.OfferId == 1).IsPremiumFeature = true;
            state.OffersTestData.AddRange(state.OffersSuppressedMerchantTestData);
            var result = await state.OfferService.GetSpecialOffers(Constants.Clients.CashRewards, Constants.Clients.Blue, OfferTypeEnum.PremiumFeature, false);

            var expectedOfferIds = new int[] { 6, 11 };
            var resultOfferIds = result.PremiumFeatureOffers.Select(offer => offer.Id);

            resultOfferIds.Should().BeEquivalentTo(expectedOfferIds);
        }

        #endregion
    }
}
