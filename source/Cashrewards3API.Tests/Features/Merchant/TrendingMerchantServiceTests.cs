using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
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
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Merchant
{
    public class TrendingMerchantServiceTests
    {
        private class TestState
        {
            public TrendingMerchantService TrendingMerchantService { get; }

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
                var trendingMerchantRepository = new Mock<ITrendingMerchantRepository>();
                trendingMerchantRepository
                    .Setup(r => r.GetMerchantsByIdListAsync(It.IsAny<List<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<bool>()))
                    .ReturnsAsync((List<int> clientIds, IEnumerable<int> merchandIds, bool isMobile) => _trendingMerchants.Where(m => clientIds.Contains(m.ClientId)));

                var awsS3Service = new Mock<IAwsS3Service>();
                awsS3Service.Setup(s3 => s3.ReadAmazonS3Data(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(trendingStoresConfigJson);

                TrendingMerchantService = new TrendingMerchantService(
                    configuration,
                    new MerchantMappingService(configuration, mapper),
                    new RedisUtilMock().Setup<PagedList<MerchantDto>>().Object,
                    Mock.Of<ICacheKey>(),
                    new CacheConfig(),
                    networkExtension.Object,
                    mapper,
                    mockDb
                        ? trendingMerchantRepository.Object
                        : new TrendingMerchantRepository(new Repository(new ShopgoDBContext(new DbConfig { ShopgoDbContext = configuration["ConnectionStrings:ShopgoDbContext"] }, configuration))),
                    awsS3Service.Object);
            }

            private const string trendingStoresConfigJson = @"{""merchantIds"": [123,124,125,126,1003434]}";

            private List<MerchantViewModel> _trendingMerchants = new List<MerchantViewModel>();

            public void GivenTrendingMerchant(MerchantViewModel merchant)
            {
                _trendingMerchants.Add(merchant);
            }

            public List<MerchantViewModel> SuppressedMerchants = MerchantTestData.GetSuppressedMerchantsViewModelTestData();
        }

        [Test]
        public async Task GetTrendingStoresForMobile_ShouldReturnMobileEnabledMerchants()
        {
            var state = new TestState();
            state.GivenTrendingMerchant(new MerchantViewModel
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

            var result = await state.TrendingMerchantService.GetTrendingStoresForMobile(Constants.Clients.CashRewards, null, 0);

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
        public async Task GetTrendingStoresForMobile_ShouldReturnPremiumMerchants()
        {
            var state = new TestState();
            state.GivenTrendingMerchant(new MerchantViewModel
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

            var result = await state.TrendingMerchantService.GetTrendingStoresForMobile(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

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
        public async Task GetTrendingStoresForMobile_ShouldReturnConjoinedPremiumMerchants_GivenMerchantsForBothClients()
        {
            var state = new TestState();
            state.GivenTrendingMerchant(new MerchantViewModel
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
            state.GivenTrendingMerchant(new MerchantViewModel
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

            var result = await state.TrendingMerchantService.GetTrendingStoresForMobile(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

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
        public async Task GetTrendingStoresForMobile_ShouldReturnMerchantsWithCashback_GivenMerchantsForWithandWithoutCashback()
        {
            var state = new TestState();
            state.GivenTrendingMerchant(new MerchantViewModel
            {
                ClientId = Constants.Clients.CashRewards,
                MerchantId = 123,
                IsMobileAppEnabled = true,
                IsFlatRate = true,
                OfferCount = 2,
                Commission = 0,
                ClientComm = 0,
                MemberComm = 0,
                RewardName = "cashback"
            });
            state.GivenTrendingMerchant(new MerchantViewModel
            {
                ClientId = Constants.Clients.Blue,
                MerchantId = 124,
                IsMobileAppEnabled = true,
                IsFlatRate = true,
                OfferCount = 2,
                Commission = 100,
                ClientComm = 5,
                MemberComm = 100,
                RewardName = "cashback"
            });

            var result = await state.TrendingMerchantService.GetTrendingStoresForMobile(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);
            result.Data.Should().BeEquivalentTo(new List<MerchantDto>
            {
                new MerchantDto
                {
                    Id = 124,
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
        [Ignore("Integration")]
        [Category("Integration")]
        public async Task GetTrendingStoresForBrowser_ShouldReturnClientCommissionString()
        {
            var state = new TestState(false);

            var result = await state.TrendingMerchantService.GetTrendingStoresForBrowser(Constants.Clients.CashRewards, null, 0);

            result.Data[0].ClientCommissionString.Should().Be("Up to 11% cashback");
        }

        [Test]
        public async Task GetTrendingStoresForBrowser_ShouldReturnSuppressedMerchant_IfNonPremiumClient()
        {
            var state = new TestState();
            state.SuppressedMerchants.ForEach(state.GivenTrendingMerchant);
            var result = await state.TrendingMerchantService.GetTrendingStoresForBrowser(Constants.Clients.CashRewards, null, 0);

            var expectedMerchantIds = new int[] { 123, 124 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);
            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }

        [Test]
        public async Task GetTrendingStoresForBrowser_ShouldNotReturnSuppressedMerchant_IfPremiumClient()
        {
            var state = new TestState();
            state.SuppressedMerchants.ForEach(state.GivenTrendingMerchant);
            var result = await state.TrendingMerchantService.GetTrendingStoresForBrowser(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

            var expectedMerchantIds = new int[] { 123, 126 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);
            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }

        [Test]
        public async Task GetTrendingStoresForMobile_ShouldReturnSuppressedMerchant_IfNonPremiumClient()
        {
            var state = new TestState();
            state.SuppressedMerchants.ForEach(state.GivenTrendingMerchant);
            var result = await state.TrendingMerchantService.GetTrendingStoresForBrowser(Constants.Clients.CashRewards, null, 0);

            var expectedMerchantIds = new int[] { 123, 124 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);
            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }

        [Test]
        public async Task GetTrendingStoresForMobile_ShouldNotReturnSuppressedMerchant_IfPremiumClient()
        {
            var state = new TestState();
            state.SuppressedMerchants.ForEach(state.GivenTrendingMerchant);
            var result = await state.TrendingMerchantService.GetTrendingStoresForBrowser(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

            var expectedMerchantIds = new int[] { 123, 126 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);
            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }
    }
}
