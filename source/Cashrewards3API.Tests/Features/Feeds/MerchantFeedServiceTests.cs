using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.Feeds.Models;
using Cashrewards3API.Features.Feeds.Service;
using Cashrewards3API.Mapper;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Feeds
{
    public class MerchantFeedServiceTests
    {
        private class TestState
        {
            public MerchantFeedService MerchantFeedService { get; }

            public Mock<IReadOnlyRepository> Repository { get; } = new();

            public TestState(bool useMockDb = true)
            {
                var config = new MapperConfiguration(cfg => cfg.AddProfile<MerchantProfile>());
                var mapper = config.CreateMapper();

                var configs = new Dictionary<string, string>
                {
                    ["Config:Transaction:CashrewardsReferAMateMerchantId"] = "1002347"
                };

                var configuration = new ConfigurationBuilder()
                    .AddJsonFile($"{Assembly.Load("Cashrewards3API").Folder()}/appsettings.Development.json", true)
                    .AddInMemoryCollection(configs)
                    .Build();

                MerchantFeedService = new MerchantFeedService(
                    new RedisUtilMock().Setup<IEnumerable<MerchantFeedModel>>().Object,
                    Mock.Of<ICacheKey>(),
                    new CacheConfig(),
                    useMockDb ? Repository.Object : TestRepository(configuration),
                    mapper,
                    configuration);
            }

            private IReadOnlyRepository TestRepository(IConfiguration configuration) =>
                new Repository(new ShopgoDBContext(new DbConfig { ShopgoDbContext = configuration["ConnectionStrings:ShopgoDbContext"] }, configuration));

            public List<MerchantFeedDataModel> MerchantData { get; } = new()
            {
                new MerchantFeedDataModel
                {
                    MerchantId = 1000467,
                    MerchantName = "Cashmere Boutique",
                    DescriptionShort = "Cashmere Boutique is the premier online boutique for the finest cashmere.",
                    HyphenatedString = "cashmere-boutique",
                    WebsiteUrl = "http://www.cashmereboutique.com",
                    ClientId = Constants.Clients.CashRewards
                },
                new MerchantFeedDataModel
                {
                    MerchantId = 1003604,
                    MerchantName = "Apple Australia",
                    DescriptionShort = "Buy Apple products from Apple Australia and earn cashback as you shop.",
                    HyphenatedString = "apple",
                    WebsiteUrl = "https://www.apple.com/au/",
                    ClientId = Constants.Clients.CashRewards
                }
            };

            public List<MerchantFeedTierDataModel> MerchantTierData { get; } = new()
            {
                new MerchantFeedTierDataModel
                {
                    MerchantId = 1000467,
                    MerchantTierId = 4481,
                    Commission = 7,
                    ClientComm = 70,
                    MemberComm = 100,
                    TierCommTypeId = 101,
                    TierDescription = "Standard Rate",
                    ClientId = Constants.Clients.CashRewards
                },
                new MerchantFeedTierDataModel
                {
                    MerchantId = 1000467,
                    MerchantTierId = 4482,
                    Commission = 9,
                    ClientComm = 70,
                    MemberComm = 100,
                    TierCommTypeId = 101,
                    TierDescription = "Special Rate",
                    ClientId = Constants.Clients.CashRewards
                },
                new MerchantFeedTierDataModel
                {
                    MerchantId = 1003604,
                    MerchantTierId = 89934,
                    Commission = 5,
                    ClientComm = 20,
                    MemberComm = 100,
                    TierCommTypeId = 101,
                    TierDescription = "Standard Rate",
                    ClientId = Constants.Clients.CashRewards
                }
            };
        }

        [Test]
        public async Task GetMerchantFeed_ShouldReturnMerchantFeedModel_GivenMerchantData()
        {
            var state = new TestState();
            state.Repository.Setup(r => r.QueryAsync<MerchantFeedDataModel>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(state.MerchantData);
            state.Repository.Setup(r => r.QueryAsync<MerchantFeedTierDataModel>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(state.MerchantTierData);

            var merchants = (await state.MerchantFeedService.GetMerchantFeed(Constants.Clients.CashRewards, null)).ToList();

            merchants.Should().BeEquivalentTo(new List<MerchantFeedModel>
            {
                new MerchantFeedModel
                {
                    MerchantId = 1000467,
                    MerchantName = "Cashmere Boutique",
                    MerchantDescription = "Cashmere Boutique is the premier online boutique for the finest cashmere.",
                    MerchantWebsite = "http://www.cashmereboutique.com",
                    MerchantTrackingUrl = "http://cashrewards.com.au/cashmere-boutique/",
                    MerchantTier = new List<MerchantFeedTierModel>
                    {
                        new MerchantFeedTierModel
                        {
                            TierDescription = "Standard Rate",
                            TierCashback = 4.9m,
                            TierCashbackType = "Percentage Value"
                        },
                        new MerchantFeedTierModel
                        {
                            TierDescription = "Special Rate",
                            TierCashback = 6.3m,
                            TierCashbackType = "Percentage Value"
                        }
                    }
                },
                new MerchantFeedModel
                {
                    MerchantId = 1003604,
                    MerchantName = "Apple Australia",
                    MerchantDescription = "Buy Apple products from Apple Australia and earn cashback as you shop.",
                    MerchantWebsite = "https://www.apple.com/au/",
                    MerchantTrackingUrl = "http://cashrewards.com.au/apple/",
                    MerchantTier = new List<MerchantFeedTierModel>
                    {
                        new MerchantFeedTierModel
                        {
                            TierDescription = "Standard Rate",
                            TierCashback = 1,
                            TierCashbackType = "Percentage Value"
                        }
                    }
                }
            });
        }

        [Test]
        public async Task GetMerchantFeed_ShouldReturnPremium_GivenPremiumMerchantData()
        {
            var state = new TestState();
            state.Repository.Setup(r => r.QueryAsync<MerchantFeedDataModel>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(state.MerchantData);
            state.Repository.Setup(r => r.QueryAsync<MerchantFeedTierDataModel>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(state.MerchantTierData);
            state.MerchantData.Add(new MerchantFeedDataModel
            {
                MerchantId = 1003604,
                MerchantName = "Apple Australia",
                DescriptionShort = "Buy Apple products from Apple Australia and earn cashback as you shop.",
                HyphenatedString = "apple",
                WebsiteUrl = "https://www.apple.com/au/",
                ClientId = Constants.Clients.Blue
            });
            state.MerchantTierData.Add(new MerchantFeedTierDataModel
            {
                MerchantId = 1003604,
                MerchantTierId = 89934,
                Commission = 5,
                ClientComm = 100,
                MemberComm = 100,
                TierCommTypeId = 101,
                TierDescription = "Standard Rate",
                ClientId = Constants.Clients.Blue
            });

            var merchants = (await state.MerchantFeedService.GetMerchantFeed(Constants.Clients.CashRewards, Constants.Clients.Blue)).ToList();

            merchants.Should().BeEquivalentTo(new List<MerchantFeedModel>
            {
                new MerchantFeedModel
                {
                    MerchantId = 1000467,
                    MerchantName = "Cashmere Boutique",
                    MerchantDescription = "Cashmere Boutique is the premier online boutique for the finest cashmere.",
                    MerchantWebsite = "http://www.cashmereboutique.com",
                    MerchantTrackingUrl = "http://cashrewards.com.au/cashmere-boutique/",
                    MerchantTier = new List<MerchantFeedTierModel>
                    {
                        new MerchantFeedTierModel
                        {
                            TierDescription = "Standard Rate",
                            TierCashback = 4.9m,
                            TierCashbackType = "Percentage Value"
                        },
                        new MerchantFeedTierModel
                        {
                            TierDescription = "Special Rate",
                            TierCashback = 6.3m,
                            TierCashbackType = "Percentage Value"
                        }
                    }
                },
                new MerchantFeedModel
                {
                    MerchantId = 1003604,
                    MerchantName = "Apple Australia",
                    MerchantDescription = "Buy Apple products from Apple Australia and earn cashback as you shop.",
                    MerchantWebsite = "https://www.apple.com/au/",
                    MerchantTrackingUrl = "http://cashrewards.com.au/apple/",
                    MerchantTier = new List<MerchantFeedTierModel>
                    {
                        new MerchantFeedTierModel
                        {
                            TierDescription = "Standard Rate",
                            TierCashback = 1,
                            TierCashbackType = "Percentage Value",
                            Premium = new MerchantFeedTierPremiumModel
                            {
                                TierCashback = 5,
                                TierCashbackType = "Percentage Value"
                            }
                        }
                    }
                }
            });
        }

        [Test]
        public async Task GetMerchantFeed_ShouldReturnPremium_GivenPremiumOnlyMerchantData()
        {
            var state = new TestState();
            var merchantData = new List<MerchantFeedDataModel>
            {
                new MerchantFeedDataModel
                {
                    MerchantId = 1003604,
                    MerchantName = "Apple Australia",
                    DescriptionShort = "Buy Apple products from Apple Australia and earn cashback as you shop.",
                    HyphenatedString = "apple",
                    WebsiteUrl = "https://www.apple.com/au/",
                    ClientId = Constants.Clients.Blue
                }
            };
            var tierData = new List<MerchantFeedTierDataModel>
            {
                new MerchantFeedTierDataModel
                {
                    MerchantId = 1003604,
                    MerchantTierId = 89934,
                    Commission = 5,
                    ClientComm = 100,
                    MemberComm = 100,
                    TierCommTypeId = 101,
                    TierDescription = "Standard Rate",
                    ClientId = Constants.Clients.Blue
                }
            };
            state.Repository.Setup(r => r.QueryAsync<MerchantFeedDataModel>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(merchantData);
            state.Repository.Setup(r => r.QueryAsync<MerchantFeedTierDataModel>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(tierData);

            var merchants = (await state.MerchantFeedService.GetMerchantFeed(Constants.Clients.CashRewards, Constants.Clients.Blue)).ToList();

            merchants.Should().BeEquivalentTo(new List<MerchantFeedModel>
            {
                new MerchantFeedModel
                {
                    MerchantId = 1003604,
                    MerchantName = "Apple Australia",
                    MerchantDescription = "Buy Apple products from Apple Australia and earn cashback as you shop.",
                    MerchantWebsite = "https://www.apple.com/au/",
                    MerchantTrackingUrl = "http://cashrewards.com.au/apple/",
                    MerchantTier = new List<MerchantFeedTierModel>
                    {
                        new MerchantFeedTierModel
                        {
                            TierDescription = "Standard Rate",
                            TierCashback = 0,
                            TierCashbackType = "Percentage Value",
                            Premium = new MerchantFeedTierPremiumModel
                            {
                                TierCashback = 5,
                                TierCashbackType = "Percentage Value"
                            }
                        }
                    }
                }
            });
        }

        [Test]
        public async Task GetMerchantFeed_ShouldNotReturnPremium_GivenPremiumHasEqualOrLessCashback()
        {
            var state = new TestState();
            state.Repository.Setup(r => r.QueryAsync<MerchantFeedDataModel>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(state.MerchantData);
            state.Repository.Setup(r => r.QueryAsync<MerchantFeedTierDataModel>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(state.MerchantTierData);
            state.MerchantData.Add(new MerchantFeedDataModel
            {
                MerchantId = 1003604,
                MerchantName = "Apple Australia",
                DescriptionShort = "Buy Apple products from Apple Australia and earn cashback as you shop.",
                HyphenatedString = "apple",
                WebsiteUrl = "https://www.apple.com/au/",
                ClientId = Constants.Clients.Blue
            });
            state.MerchantTierData.Add(new MerchantFeedTierDataModel
            {
                MerchantId = 1003604,
                MerchantTierId = 89934,
                Commission = 5,
                ClientComm = 10,
                MemberComm = 100,
                TierCommTypeId = 101,
                TierDescription = "Standard Rate",
                ClientId = Constants.Clients.Blue
            });

            var merchants = (await state.MerchantFeedService.GetMerchantFeed(Constants.Clients.CashRewards, Constants.Clients.Blue)).ToList();

            merchants.Single(m => m.MerchantId == 1003604).MerchantTier.ToList().ForEach(t => t.Premium.Should().BeNull());
        }

        private class MerchantData
        {
            public List<MerchantFeedModel> MerchantFeed { get; set; }
        }

        //[Test]
        public async Task IntegrationTest_GetMerchantFeed_ShouldReturnSameDataAsOldFeedEndpoint_GivenNoPremiumClientId()
        {
            var state = new TestState(false);

            var merchants = await state.MerchantFeedService.GetMerchantFeed(Constants.Clients.CashRewards, null);
            var merchantsList = merchants.ToList();
            var merchantsDict = merchantsList.ToDictionary(x => x.MerchantId);

            var existingFeed = TestDataLoader.Load<MerchantData>("Features/Feeds/JSON/merchant-feed-staging.json");
            var existingFeedDict = existingFeed.MerchantFeed.ToDictionary(x => x.MerchantId);

            var extraMerchants = merchantsList.Where(m => !existingFeedDict.ContainsKey(m.MerchantId));
            foreach (var m in extraMerchants)
            {
                Console.WriteLine($"extra -> {m.MerchantId} - {m.MerchantName}");
            }

            var missingMerchants = existingFeed.MerchantFeed.Where(m => !merchantsDict.ContainsKey(m.MerchantId));
            foreach (var m in missingMerchants)
            {
                Console.WriteLine($"missing -> {m.MerchantId} - {m.MerchantName}");
            }

            extraMerchants.Should().BeEmpty();
            missingMerchants.Should().BeEmpty();
            merchantsList.Count.Should().Be(existingFeed.MerchantFeed.Count);

            foreach (var existingFeedMerchant in existingFeed.MerchantFeed)
            {
                Console.WriteLine(existingFeedMerchant.MerchantId);
                merchantsDict[existingFeedMerchant.MerchantId].Should().BeEquivalentTo(existingFeedMerchant);
            }
        }
    }
}
