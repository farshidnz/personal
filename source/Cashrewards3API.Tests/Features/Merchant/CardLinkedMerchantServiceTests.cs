using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.CardLinkedMerchant;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Merchant
{
    public class CardLinkedMerchantServiceTests
    {
        public class TestState
        {
            public CardLinkedMerchantService CardLinkedMerchantService { get; }

            public TestState(bool useStagingDb = false)
            {
                var configs = new Dictionary<string, string>
                {
                    ["Config:CustomTrackingMerchantList"] = "1001330",
                    ["Config:MerchantTierCommandTypeId"] = "101"
                };

                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(configs)
                    .AddJsonFile($"{Assembly.Load("Cashrewards3API").Folder()}/appsettings.Development.json", true)
                    .Build();

                var cacheKey = new Mock<ICacheKey>();

                var shopGoDBContext = new ShopgoDBContext(new DbConfig { ShopgoDbContext = configuration["ConnectionStrings:ShopgoDbContext"] }, configuration);
                var cacheConfig = new CacheConfig();
                var repository = CreateRepository(useStagingDb, shopGoDBContext);

                CardLinkedMerchantService = new CardLinkedMerchantService(
                    configuration,
                    cacheKey.Object,
                    new RedisUtilMock().Setup<List<CardLinkedMerchantDto>>().Object,
                    cacheConfig,
                    repository);
            }

            private IRepository CreateRepository(bool useStagingDb, ShopgoDBContext shopGoDBContext)
            {
                if (useStagingDb)
                {
                    return new Repository(shopGoDBContext);
                }

                var repositoryMock = new Mock<IRepository>();
                repositoryMock.Setup(o => o.Query<CardLinkedMerchantFullViewModel>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>()))
                    .ReturnsAsync(CardLinkedMerchantsTestData);
                repositoryMock.Setup(o => o.Query<CardLinkedMerchantViewModel>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>()))
                    .ReturnsAsync((string sql, object param, int? timeout) => GetMerchantData(sql, param,null));

                return repositoryMock.Object;
            }

            private List<CardLinkedMerchantViewModel> GetMerchantData(string sql, object param, int? timeout)
            {
                var p = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(
                    JsonConvert.SerializeObject(param));
                return LinkedMerchantsTestData.Where(m => p["ClientIds"].Contains(m.ClientId)).ToList();
            }

            private List<CardLinkedMerchantFullViewModel> CardLinkedMerchantsTestData =
                new List<CardLinkedMerchantFullViewModel>
                {
                };

            public List<CardLinkedMerchantViewModel> LinkedMerchantsTestData = new List<CardLinkedMerchantViewModel>
            {
                new CardLinkedMerchantViewModel
                {
                    MerchantId = 1,
                    ClientId = Constants.Clients.CashRewards,
                    MerchantHyphenatedString = "dan-murphys",
                    Commission = 1.25m,
                    TierCommTypeId = 101,
                    IsFlatRate = true,
                },
                new CardLinkedMerchantViewModel
                {
                    MerchantId = 2,
                    ClientId = Constants.Clients.MoneyMe,
                    MerchantHyphenatedString = "david-jones",
                    Commission = 8,
                    TierCommTypeId = 101,
                }
            };

            public List<CardLinkedMerchantViewModel> LinkedSuppressedMerchantsTestData = MerchantTestData.GetLinkedSuppressedMerchantsTestData();
        }

        [Test]
        public async Task GetCardLinkedMerchantsAsync_ShouldReturnCashrewardsMerchants()
        {
            var state = new TestState();

            var merchants =
                await state.CardLinkedMerchantService.GetCardLinkedMerchantsAsync(Constants.Clients.CashRewards, null,
                    0);

            merchants.Should().BeEquivalentTo(new List<CardLinkedMerchantDto>
            {
                new CardLinkedMerchantDto
                {
                    MerchantId = 1,
                    ClientId = Constants.Clients.CashRewards,
                    MerchantHyphenatedString = "dan-murphys",
                    CommissionString = "1.25% cashback",
                    Channels = new List<string> {"Online"},
                    Commission = 1.25m,
                    CommissionType = Constants.MerchantCommissionType.Percent,
                    IsFlatRate = true,
                }
            });
        }

        [Test]
        public async Task GetCardLinkedMerchantsAsync_ShouldReturnMoneyMeMerchants()
        {
            var state = new TestState();

            var merchants =
                await state.CardLinkedMerchantService.GetCardLinkedMerchantsAsync(Constants.Clients.MoneyMe, null, 0);

            merchants.Should().BeEquivalentTo(new List<CardLinkedMerchantDto>
            {
                new CardLinkedMerchantDto
                {
                    MerchantId = 2,
                    ClientId = Constants.Clients.MoneyMe,
                    MerchantHyphenatedString = "david-jones",
                    CommissionString = "8% cashback",
                    Channels = new List<string> {"Online"},
                    Commission = 8,
                    CommissionType = Constants.MerchantCommissionType.Percent
                }
            });
        }

        [Test]
        public async Task
            GetCardLinkedMerchantsAsync_ShouldGroupMerchantsByHyphenatedString_GivenMerchantsWithTheSameHyphenatedString()
        {
            var state = new TestState();
            state.LinkedMerchantsTestData.Add(new CardLinkedMerchantViewModel
            {
                MerchantId = 6,
                ClientId = Constants.Clients.CashRewards,
                MerchantHyphenatedString = "dan-murphys",
                InStore = true,
                Commission = 1.25m
            });

            var merchants =
                await state.CardLinkedMerchantService.GetCardLinkedMerchantsAsync(Constants.Clients.CashRewards, null,
                    0);

            merchants.Should().BeEquivalentTo(new List<CardLinkedMerchantDto>
            {
                new CardLinkedMerchantDto
                {
                    MerchantId = 1,
                    ClientId = Constants.Clients.CashRewards,
                    MerchantHyphenatedString = "dan-murphys",
                    CommissionString = "1.25% cashback",
                    Channels = new List<string> {"Online", "In-Store"},
                    Commission = 1.25m,
                    CommissionType = Constants.MerchantCommissionType.Percent,
                    IsFlatRate = true
                }
            });
        }

        [Test]
        public async Task
            GetCardLinkedMerchantsAsync_ShouldReturnStandardAndPremiumMerchants_GivenPremiumMerchantAndPremiumClientId()
        {
            var state = new TestState();
            state.LinkedMerchantsTestData.Add(new CardLinkedMerchantViewModel
            {
                MerchantId = 3,
                ClientId = Constants.Clients.Blue,
                MerchantHyphenatedString = "rebel-sport",
                Commission = 9,
                TierCommTypeId = 100,
                IsFlatRate = true
            });

            var merchants =
                await state.CardLinkedMerchantService.GetCardLinkedMerchantsAsync(Constants.Clients.CashRewards,
                    Constants.Clients.Blue, 0);

            merchants.Should().BeEquivalentTo(new List<CardLinkedMerchantDto>
            {
                new CardLinkedMerchantDto
                {
                    MerchantId = 1,
                    ClientId = Constants.Clients.CashRewards,
                    MerchantHyphenatedString = "dan-murphys",
                    CommissionString = "1.25% cashback",
                    Channels = new List<string> {"Online"},
                    Commission = 1.25m,
                    CommissionType = Constants.MerchantCommissionType.Percent,
                    IsFlatRate = true
                },
                new CardLinkedMerchantDto
                {
                    MerchantId = 3,
                    ClientId = Constants.Clients.Blue,
                    MerchantHyphenatedString = "rebel-sport",
                    CommissionString = "$9 cashback",
                    Channels = new List<string> {"Online"},
                    CommissionType = Constants.MerchantCommissionType.Dollar,
                    Commission = 9m,
                    IsFlatRate = true,
                    Premium = new PremiumCardLinkedMerchant
                    {
                        CommissionString = "$9 cashback",
                        CommissionType = Constants.MerchantCommissionType.Dollar,
                        Commission = 9m,
                        IsFlatRate = true
                    }
                }
            });
        }

        [Test]
        public async Task GetCardLinkedMerchantsAsync_ShouldReturnCojoinedStandardAndPremiumMerchants_GivenMerchantIsForBothClients()
        {
            var state = new TestState();
            state.LinkedMerchantsTestData.Add(new CardLinkedMerchantViewModel
            {
                MerchantId = 6,
                ClientId = Constants.Clients.Blue,
                MerchantHyphenatedString = "dan-murphys",
                Commission = 20,
                TierCommTypeId = 101,
                IsFlatRate = true,
            });

            var merchants = await state.CardLinkedMerchantService.GetCardLinkedMerchantsAsync(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

            merchants.Should().BeEquivalentTo(new List<CardLinkedMerchantDto>
            {
                new CardLinkedMerchantDto
                {
                    MerchantId = 1,
                    ClientId = Constants.Clients.CashRewards,
                    MerchantHyphenatedString = "dan-murphys",
                    CommissionString = "1.25% cashback",
                    Channels = new List<string> { "Online" },
                    Commission = 1.25m,
                    CommissionType = Constants.MerchantCommissionType.Percent,
                    IsFlatRate = true,
                    Premium = new PremiumCardLinkedMerchant
                    {
                        CommissionString = "20% cashback",
                        Commission = 20m,
                        CommissionType = Constants.MerchantCommissionType.Percent,
                        IsFlatRate = true
                    }
                }
            });
        }

        [Test]
        public async Task GetCardLinkedMerchantsAsync_ShouldReturnMerchantsOnlyWithCashback_GivenWithandWithoutCashback()
        {
            var state = new TestState();
            state.LinkedMerchantsTestData.Add(new CardLinkedMerchantViewModel
            {
                MerchantId = 6,
                ClientId = Constants.Clients.CashRewards,
                MerchantHyphenatedString = "dan-murphys",
                Commission = 20,
                TierCommTypeId = 101,
                IsFlatRate = true,
            });

            state.LinkedMerchantsTestData.Add(new CardLinkedMerchantViewModel
            {
                MerchantId = 7,
                ClientId = Constants.Clients.CashRewards,
                MerchantHyphenatedString = "dan-murphys",
                Commission = 0,
                TierCommTypeId = 101,
                IsFlatRate = true,
            });

            var merchants = await state.CardLinkedMerchantService.GetCardLinkedMerchantsAsync(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

            merchants.Should().BeEquivalentTo(new List<CardLinkedMerchantDto>
            {
                new CardLinkedMerchantDto
                {
                    MerchantId = 1,
                    ClientId = Constants.Clients.CashRewards,
                    MerchantHyphenatedString = "dan-murphys",
                    CommissionString = "1.25% cashback",
                    Channels = new List<string> { "Online" },
                    Commission = 1.25m,
                    CommissionType = Constants.MerchantCommissionType.Percent,
                    IsFlatRate = true,
                    Premium = null
                }
            });
        }

        [Test]
        public async Task GetCardLinkedMerchantsAsync_ShouldReturnSuppressedMerchants_IfNonPremiumClient()
        {
            var state = new TestState();
            state.LinkedMerchantsTestData.AddRange(state.LinkedSuppressedMerchantsTestData);

            var merchants = await state.CardLinkedMerchantService.GetCardLinkedMerchantsAsync(Constants.Clients.CashRewards, null, 0);

            var expectedMerchantIds = new int[] { 1, 3, 4 };
            var merchantIds = merchants.Select(merchant => merchant.MerchantId);

            merchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }

        [Test]
        public async Task GetCardLinkedMerchantsAsync_ShouldNotReturnSuppressedMerchants_IfPremiumClient()
        {
            var state = new TestState();
            state.LinkedMerchantsTestData.AddRange(state.LinkedSuppressedMerchantsTestData);

            var merchants = await state.CardLinkedMerchantService.GetCardLinkedMerchantsAsync(Constants.Clients.CashRewards, Constants.Clients.Blue, 0);

            var expectedMerchantIds = new int[] { 1, 4, 6 };
            var merchantIds = merchants.Select(merchant => merchant.MerchantId);

            merchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }
    }
}
