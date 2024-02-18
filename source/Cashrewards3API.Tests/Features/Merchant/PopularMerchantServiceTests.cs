using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Merchant.Repository;
using Cashrewards3API.Mapper;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Merchant
{
    public class PopularMerchantServiceTests
    {
        public class TestState
        {
            public PopularMerchantService PopularMerchantService { get; }
            public Mock<IFeatureToggle> featureToggle { get; set; }

            public TestState(bool mockDb = true)
            {
                var configs = new Dictionary<string, string>
                {
                    ["Config:PopularStoreInfoTable"] = "trending-stores-api-PopularStoresPublishFacade-17JZGKSW19WFD",
                    ["Config:CustomTrackingMerchantList"] = "1001330"
                };

                var configuration = new ConfigurationBuilder()
                    .AddJsonFile($"{Assembly.Load("Cashrewards3API").Folder()}/appsettings.Development.json", true)
                    .AddInMemoryCollection(configs)
                    .Build();

                var networkExtension = new Mock<INetworkExtension>();
                var config = new MapperConfiguration(cfg => cfg.AddProfile<MerchantProfile>());
                var mapper = config.CreateMapper();
                var popularMerchantRepository = new Mock<IPopularMerchantRepository>();
                popularMerchantRepository
                    .Setup(r => r.GetPopularMerchantsFromDbAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
                    .ReturnsAsync((int clientId, IEnumerable<int> merchandIds) => _popularMerchants.Where(m => m.ClientId == clientId));

                featureToggle = new Mock<IFeatureToggle>();
                featureToggle.Setup(f => f.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(false);

                PopularMerchantService = new PopularMerchantService(
                    configuration,
                    new MerchantMappingService(configuration, mapper),
                    _amazonDynamoDb.Object,
                    new RedisUtilMock().Setup<PagedList<MerchantDto>>().Object,
                    Mock.Of<ICacheKey>(),
                    new CacheConfig(),
                    networkExtension.Object,
                    mapper,
                    mockDb
                        ? popularMerchantRepository.Object
                        : new PopularMerchantRepository(new Repository(new ShopgoDBContext(new DbConfig { ShopgoDbContext = configuration["ConnectionStrings:ShopgoDbContext"] }, configuration))),featureToggle.Object);
            }

            private Mock<IAmazonDynamoDB> _amazonDynamoDb = new();

            private List<MerchantViewModel> _popularMerchants = new List<MerchantViewModel>();

            public List<MerchantViewModel> SuppressedMerchants = MerchantTestData.GetSuppressedMerchantsViewModelTestData();

            public void GivenPopularMerchant(MerchantViewModel merchant)
            {
                _popularMerchants.Add(merchant);
                GivenPopularMerchantInDynamoDb(_popularMerchants.Select(m => m.MerchantId).ToArray());
            }

            public void GivenPopularMerchantInDynamoDb(params int[] merchantIds)
            {
                _amazonDynamoDb
                    .Setup(d => d.GetItemAsync(It.IsAny<GetItemRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new GetItemResponse
                    {
                        Item = new Dictionary<string, AttributeValue>
                        {
                            ["orderedMerchantIds"] = new AttributeValue()
                            {
                                L = merchantIds.Distinct().Select(id => new AttributeValue { N = id.ToString() }).ToList()
                            }
                        }
                    });
            }
            public void MockFeatureToggle(bool on) =>
                featureToggle.Setup(p => p.IsEnabled(It.Is<string>(key => key == FeatureFlags.IS_MERCHANT_PAUSED))).Returns(on);
        }

        [Test]
        public async Task GetPopularMerchantsForMobileAsync_ShouldReturnMobileEnabledMerchants()
        {
            var state = new TestState();
            state.GivenPopularMerchant(new MerchantViewModel
            {
                ClientId = Constants.Clients.CashRewards,
                MerchantId = 123,
                IsMobileAppEnabled = true,
                IsFlatRate = true,
                OfferCount = 2,
                Commission = 100,
                ClientComm = 3,
                MemberComm = 100,
                RewardName = "cashback"
            });

            var result = await state.PopularMerchantService.GetPopularMerchantsForMobileAsync(Constants.Clients.CashRewards, null);

            result.Data.Should().BeEquivalentTo(new List<MerchantDto>
            {
                new MerchantDto
                {
                    Id = 123,
                    IsFlatRate = true,
                    OfferCount = 2,
                    CommissionType = string.Empty,
                    RewardType = "Cashback",
                    MobileTrackingType = "0",
                    MobileTrackingNetwork = string.Empty,
                    Commission = 3,
                    ClientCommissionString = "3% cashback"
                }
            });
        }

        [Test]
        public async Task GetPopularMerchantsForMobileAsync_ShouldReturnPremiumMerchants()
        {
            var state = new TestState();
            state.GivenPopularMerchant(new MerchantViewModel
            {
                ClientId = Constants.Clients.Blue,
                MerchantId = 123,
                IsMobileAppEnabled = true,
                IsFlatRate = true,
                OfferCount = 2,
                Commission = 100,
                ClientComm = 5,
                MemberComm = 100,
                RewardName = "cashback"
            });

            var result = await state.PopularMerchantService.GetPopularMerchantsForMobileAsync(Constants.Clients.CashRewards, Constants.Clients.Blue);

            result.Data.Should().BeEquivalentTo(new List<MerchantDto>
            {
                new MerchantDto
                {
                    Id = 123,
                    IsFlatRate = true,
                    OfferCount = 2,
                    CommissionType = string.Empty,
                    RewardType = "Cashback",
                    MobileTrackingType = "0",
                    MobileTrackingNetwork = string.Empty,
                    Commission = 5,
                    ClientCommissionString = "5% cashback",
                    Premium = new PremiumMerchant
                    {
                        IsFlatRate = true,
                        Commission = 5,
                        ClientCommissionString = "5% cashback"
                    }
                }
            });
        }

        [Test]
        public async Task GetPopularMerchantsForMobileAsync_ShouldReturnConjoinedPremiumMerchants_GivenMerchantsForBothClients()
        {
            var state = new TestState();
            state.GivenPopularMerchant(new MerchantViewModel
            {
                ClientId = Constants.Clients.CashRewards,
                MerchantId = 123,
                IsMobileAppEnabled = true,
                IsFlatRate = true,
                OfferCount = 2,
                Commission = 100,
                ClientComm = 3,
                MemberComm = 100,
                RewardName = "cashback"
            });
            state.GivenPopularMerchant(new MerchantViewModel
            {
                ClientId = Constants.Clients.Blue,
                MerchantId = 123,
                IsMobileAppEnabled = true,
                IsFlatRate = true,
                OfferCount = 2,
                Commission = 100,
                ClientComm = 5,
                MemberComm = 100,
                RewardName = "cashback"
            });

            var result = await state.PopularMerchantService.GetPopularMerchantsForMobileAsync(Constants.Clients.CashRewards, Constants.Clients.Blue);

            result.Data.Should().BeEquivalentTo(new List<MerchantDto>
            {
                new MerchantDto
                {
                    Id = 123,
                    IsFlatRate = true,
                    OfferCount = 2,
                    CommissionType = string.Empty,
                    RewardType = "Cashback",
                    MobileTrackingType = "0",
                    MobileTrackingNetwork = string.Empty,
                    Commission = 3,
                    ClientCommissionString = "3% cashback",
                    Premium = new PremiumMerchant
                    {
                        IsFlatRate = true,
                        Commission = 5,
                        ClientCommissionString = "5% cashback"
                    }
                }
            });
        }

        [Test]
        public async Task GetPopularMerchantsForMobileAsync_ShouldReturnStoreOnlyWithCashback_GivenBoth()
        {
            var state = new TestState();
            state.GivenPopularMerchant(new MerchantViewModel
            {
                ClientId = Constants.Clients.CashRewards,
                MerchantId = 123,
                IsMobileAppEnabled = true,
                IsFlatRate = true,
                OfferCount = 2,
                Commission = 100,
                ClientComm = 3,
                MemberComm = 100,
                RewardName = "cashback"
            });
            state.GivenPopularMerchant(new MerchantViewModel
            {
                ClientId = Constants.Clients.CashRewards,
                MerchantId = 124,
                IsMobileAppEnabled = true,
                IsFlatRate = true,
                OfferCount = 2,
                Commission = 0,
                ClientComm = 0,
                MemberComm = 0,
                RewardName = "cashback"
            });

            var result = await state.PopularMerchantService.GetPopularMerchantsForMobileAsync(Constants.Clients.CashRewards, Constants.Clients.Blue);

            result.Data.Should().BeEquivalentTo(new List<MerchantDto>
            {
                new MerchantDto
                {
                    Id = 123,
                    IsFlatRate = true,
                    OfferCount = 2,
                    CommissionType = string.Empty,
                    RewardType = "Cashback",
                    MobileTrackingType = "0",
                    MobileTrackingNetwork = string.Empty,
                    Commission = 3,
                    ClientCommissionString = "3% cashback",
                    Premium = null
                }
            });
        }

        [Test]
        [Ignore("Integration")]
        [Category("Integration")]
        public async Task GetPopularMerchantsForBrowserAsync_ShouldReturnClientCommissionString()
        {
            var state = new TestState(false);
            state.GivenPopularMerchantInDynamoDb(1003434);

            var result = await state.PopularMerchantService.GetPopularMerchantsForBrowserAsync(Constants.Clients.CashRewards, null);

            result.Data[0].ClientCommissionString.Should().Be("Up to 11% cashback");
        }

        [Test]
        public async Task GetPopularMerchantsForBrowserAsync_ShouldReturnSuppressedMerchants_IfNonPremiumClient()
        {
            var state = new TestState();
            state.SuppressedMerchants.ForEach(state.GivenPopularMerchant);

            var result = await state.PopularMerchantService.GetPopularMerchantsForBrowserAsync(Constants.Clients.CashRewards, null);

            var expectedMerchantIds = new int[] { 123, 124 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);
            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);

        }

        [Test]
        public async Task GetPopularMerchantsForBrowserAsync_ShouldNotReturnSuppressedMerchants_IfPremiumClient()
        {
            var state = new TestState();
            state.SuppressedMerchants.ForEach(state.GivenPopularMerchant);

            var result = await state.PopularMerchantService.GetPopularMerchantsForBrowserAsync(Constants.Clients.CashRewards, Constants.Clients.Blue);

            var expectedMerchantIds = new int[] { 123, 126 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);
            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }

        [Test]
        public async Task GetPopularMerchantsForMobileAsync_ShouldReturnSuppressedMerchants_IfNonPremiumClient()
        {
            var state = new TestState();
            state.SuppressedMerchants.ForEach(state.GivenPopularMerchant);

            var result = await state.PopularMerchantService.GetPopularMerchantsForMobileAsync(Constants.Clients.CashRewards, null);

            var expectedMerchantIds = new int[] { 123, 124 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);
            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);

        }

        [Test]
        public async Task GetPopularMerchantsForMobileAsync_ShouldNotReturnSuppressedMerchants_IfPremiumClient()
        {
            var state = new TestState();
            state.SuppressedMerchants.ForEach(state.GivenPopularMerchant);

            var result = await state.PopularMerchantService.GetPopularMerchantsForMobileAsync(Constants.Clients.CashRewards, Constants.Clients.Blue);

            var expectedMerchantIds = new int[] { 123, 126 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);
            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }
    }
}
