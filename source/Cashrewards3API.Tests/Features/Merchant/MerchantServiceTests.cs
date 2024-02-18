using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Merchant
{
    public class MerchantServiceTests
    {
        public class TestState
        {
            public MerchantService MerchantService { get; }
            public Mock<IRepository> Repository { get; set; }
            public int InstoreNetworkId => 22;
            public Mock<IFeatureToggle> featureToggle { get; set; }

            public TestState()
            {
                var configs = new Dictionary<string, string>
                {
                    ["Config:CustomTrackingMerchantList"] = "1001330",
                    ["Config:MerchantTierCommandTypeId"] = "101"
                };

                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(configs)
                    .Build();

                Repository = new Mock<IRepository>();
                Repository
                    .Setup(r => r.QueryAsync<MerchantHyphenatedStringModel>(It.IsAny<string>(), It.IsAny<object>()))
                    .ReturnsAsync(_hyphenatedStringModels);
                Repository
                    .Setup(r => r.QueryAsync<MerchantFullView>(It.IsAny<string>(), It.IsAny<object>()))
                    .ReturnsAsync(_merchantFullViews);
                Repository
                    .Setup(r => r.Query<MerchantFullView>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>()))
                    .ReturnsAsync(() => _merchantFullViews.Where(m => m.ClientId == Constants.Clients.Blue).ToList());
                Repository
                    .Setup(r => r.QueryAsync<MerchantTierView>(It.IsAny<string>(), It.IsAny<object>()))
                    .ReturnsAsync(_merchantTierViews);
                Repository
                    .Setup(r => r.QueryAsync<MerchantViewModel>(It.IsAny<string>(), It.IsAny<object>()))
                    .ReturnsAsync(_merchantViewModels);

                var config = new MapperConfiguration(cfg => cfg.AddMaps("Cashrewards3API"));
                var mapper = config.CreateMapper();

                var network = new Mock<INetworkExtension>();
                network.Setup(n => n.IsInStoreNetwork(It.Is<int>(id => id == InstoreNetworkId))).Returns(true);

                featureToggle = new Mock<IFeatureToggle>();
                featureToggle.Setup(f => f.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(false);

                MerchantService = new MerchantService(
                    configuration,
                    new MerchantMappingService(configuration, mapper),
                    new RedisUtilMock()
                        .Setup<PagedList<AllStoresMerchantModel>>()
                        .Setup<PagedList<MerchantBundleBasicModel>>()
                        .Object,
                    Mock.Of<ICacheKey>(),
                    Mock.Of<CacheConfig>(),
                    network.Object,
                    mapper,
                    Repository.Object, 
                    featureToggle.Object);
            }

            private readonly List<MerchantHyphenatedStringModel> _hyphenatedStringModels = new();

            private readonly List<MerchantFullView> _merchantFullViews = new();

            private readonly List<MerchantViewModel> _merchantViewModels = new();

            private readonly List<MerchantTierView> _merchantTierViews = new();

            public List<MerchantFullView> SuppressedMerchants = MerchantTestData.GetSuppressedMerchantsTestData();

            public void GivenMerchant(MerchantFullView merchantFullView)
            {
                _hyphenatedStringModels.Add(new MerchantHyphenatedStringModel {
                    HyphenatedString = merchantFullView.HyphenatedString,
                    ClientId = merchantFullView.ClientId,
                    ClientCommission = merchantFullView.ClientCommission,
                    IsPaused=merchantFullView.IsPaused
                });
                _merchantFullViews.Add(merchantFullView);

                _merchantViewModels.Add(new MerchantViewModel { MerchantId = merchantFullView.MerchantId, MerchantName = merchantFullView.MerchantName, IsPremiumDisabled = merchantFullView.IsPremiumDisabled, ClientId = merchantFullView.ClientId });
            }

            public void GivenMerchantViewModel(MerchantFullView merchantFullView)
            {

            }

            public void GivenTiers(MerchantTierView tier)
            {
                _merchantTierViews.Add(tier);
            }

            public void MockFeatureToggle(bool on) =>
            featureToggle.Setup(p => p.IsEnabled(It.Is<string>(key => key == FeatureFlags.IS_MERCHANT_PAUSED))).Returns(on);
        }

        private IEnumerable<MerchantBasicModel> FlatMapMerchantBundleBasicModel(MerchantBundleBasicModel model)
        {
            var returnModels = new List<MerchantBasicModel>();
            if (model.Online != null)
            {
                returnModels.Add(model.Online);
            }

            return returnModels;
        }



        #region GetMerchantBundleByFilterAsync

        [Test]
        public async Task GetMerchantBundleByFilterAsync_ShouldReturnMappedOnlineMerchant_GivenOnlineMerchant()
        {
            var state = new TestState();
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                MerchantId = 123,
                MerchantName = "Lucky",
                HyphenatedString = "lucky-pet-supplies",
                RegularImageUrl = "http://image",
                DescriptionShort = "short desc",
                DescriptionLong = "long desc",
                IsFlatRate = true,
                TierCommTypeId = 101,
                TierTypeId = 101,
                MobileAppTrackingType = 1,
                NotificationMsg = "note msg",
                ApprovalTime = "Up to 11 Days",
                BasicTerms = "basic terms",
                ExtentedTerms = "extended terms",
                NetworkId = 1,
                ConfirmationMsg = "cofirm msg",
                TrackingTime = "90 days",
                ClientComm = 100,
                MemberComm = 100,
                Commission = 20,
                ReferenceName = "reference",
                MerchantBadgeCode = "badge",
                MobileEnabled = true,
                DesktopEnabled = true,
                IsMobileAppEnabled = true,
                BackgroundImageUrl = "http://background-image",
                BannerImageUrl = "http://banner-image",
                MobileTrackingNetwork = 1
            });

            var result = await state.MerchantService.GetMerchantBundleByFilterAsync(new MerchantRequestInfoModel 
            {
                ClientId = Constants.Clients.CashRewards
            });

            var merchantBundle = result.Data;
            merchantBundle.Should().BeEquivalentTo(new List<MerchantBundleBasicModel>
            {
                new MerchantBundleBasicModel
                {
                    Online = new MerchantBasicModel
                    {
                        ApprovalTime = 11,
                        BackgroundImageUrl = "http://background-image",
                        BannerImageUrl = "http://banner-image",
                        CashbackGuidelines = "basic terms",
                        IsFlatRate = true,
                        Commission = 20,
                        CommissionType = "percent",
                        CommissionString = "20%",
                        Description = "long desc",
                        DesktopEnabled = true,
                        HyphenatedString = "lucky-pet-supplies",
                        Id = 123,
                        IsCustomTracking = false,
                        LogoUrl = "http://image",
                        MerchantBadge = "badge",
                        MobileTrackingNetwork = "",
                        MobileTrackingType = "ExternalBrowser",
                        Name = "Lucky",
                        OfferCount = 0M,
                        RewardType = "Cashback",
                        SpecialTerms = "extended terms",
                        TrackingTimeMax = 90
                    },
                    Offline = new List<CardLinkedBasicMerchantModel>()
                }
            });
        }

        [Test]
        public async Task GetMerchantBundleByFilterAsync_ShouldReturnPremiumOnlineDetails_GivenPremiumOnlineMerchant()
        {
            var state = new TestState();
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.Blue,
                HyphenatedString = "lucky-pet-supplies",
                TierCommTypeId = 101,
                IsFlatRate = true,
                Commission = 10,
                ClientComm = 100,
                MemberComm = 100
            });

            var result = await state.MerchantService.GetMerchantBundleByFilterAsync(new MerchantRequestInfoModel
            {
                ClientId = Constants.Clients.CashRewards,
                PremiumClientId = Constants.Clients.Blue
            });

            var merchantBundle = result.Data;
            merchantBundle.Should().BeEquivalentTo(new List<MerchantBundleBasicModel>
            {
                new MerchantBundleBasicModel
                {
                    Online = new MerchantBasicModel
                    {
                        CommissionType = "percent",
                        HyphenatedString = "lucky-pet-supplies",
                        MobileTrackingNetwork = "",
                        MobileTrackingType = "InAppBrowser",
                        RewardType = "Cashback",
                        Commission = 10m,
                        IsFlatRate = true,
                        CommissionString = "10%",
                        Premium = new MerchantBasicPremiumModel
                        {
                            Commission = 10m,
                            IsFlatRate = true,
                            CommissionString = "10%"
                        }
                    },
                    Offline = new List<CardLinkedBasicMerchantModel>()
                }
            });
        }

        [Test]
        public async Task GetMerchantBundleByFilterAsync_ShouldReturnConjoinedPremiumOnlineDetails_GivenOnlineMerchantForStandardAndPremiumClients()
        {
            var state = new TestState();
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "my-cosmetic-clinic",
                TierCommTypeId = 101,
                ClientComm = 100,
                MemberComm = 100,
                Commission = 8,
                IsFlatRate = true
            });
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.Blue,
                HyphenatedString = "my-cosmetic-clinic",
                TierCommTypeId = 101,
                IsFlatRate = true,
                Commission = 15,
                ClientComm = 100,
                MemberComm = 100
            });

            var result = await state.MerchantService.GetMerchantBundleByFilterAsync(new MerchantRequestInfoModel
            {
                ClientId = Constants.Clients.CashRewards,
                PremiumClientId = Constants.Clients.Blue
            });

            var merchantBundle = result.Data;
            merchantBundle.Should().BeEquivalentTo(new List<MerchantBundleBasicModel>
            {
                new MerchantBundleBasicModel
                {
                    Online = new MerchantBasicModel
                    {
                        HyphenatedString = "my-cosmetic-clinic",
                        CommissionType = "percent",
                        MobileTrackingNetwork = "",
                        MobileTrackingType = "InAppBrowser",
                        RewardType = "Cashback",
                        Commission = 8,
                        CommissionString = "8%",
                        IsFlatRate = true,
                        Premium = new MerchantBasicPremiumModel
                        {
                            Commission = 15m,
                            IsFlatRate = true,
                            CommissionString = "15%"
                        }
                    },
                    Offline = new List<CardLinkedBasicMerchantModel>()
                }
            });
        }

        [Test]
        public async Task GetMerchantBundleByFilterAsync_ShouldReturnMappedOfflineMerchant_GivenOfflineMerchant()
        {
            var state = new TestState();
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                MerchantId = 123,
                MerchantName = "Lucky",
                HyphenatedString = "lucky-pet-supplies",
                RegularImageUrl = "http://image",
                DescriptionShort = "short desc",
                DescriptionLong = "long desc",
                IsFlatRate = true,
                TierCommTypeId = 101,
                TierTypeId = 101,
                MobileAppTrackingType = 1,
                NotificationMsg = "note msg",
                ApprovalTime = "Up to 11 Days",
                BasicTerms = "basic terms",
                ExtentedTerms = "extended terms",
                NetworkId = state.InstoreNetworkId,
                ConfirmationMsg = "cofirm msg",
                TrackingTime = "90 days",
                ClientComm = 100,
                MemberComm = 100,
                Commission = 20,
                ReferenceName = "reference",
                MerchantBadgeCode = "badge",
                MobileEnabled = true,
                DesktopEnabled = true,
                IsMobileAppEnabled = true,
                BackgroundImageUrl = "http://background-image",
                BannerImageUrl = "http://banner-image",
                MobileTrackingNetwork = 1
            });
            state.GivenTiers(new MerchantTierView
            {
                TierCommTypeId = 100,
                TierSpecialTerms = "You must spend a minimum of $20 to be eligable to this offer"
            });

            var result = await state.MerchantService.GetMerchantBundleByFilterAsync(new MerchantRequestInfoModel
            {
                ClientId = Constants.Clients.CashRewards
            });

            var merchantBundle = result.Data;
            merchantBundle.Should().BeEquivalentTo(new List<MerchantBundleBasicModel>
            {
                new MerchantBundleBasicModel
                {
                    Offline = new List<CardLinkedBasicMerchantModel>
                    {
                        new CardLinkedBasicMerchantModel
                        {
                            ApprovalTime = 11,
                            BackgroundImageUrl = "http://background-image",
                            BannerImageUrl = "http://banner-image",
                            CashbackGuidelines = "basic terms",
                            IsFlatRate = true,
                            Commission = 20,
                            CommissionType = "percent",
                            CommissionString = "20%",
                            Description = "long desc",
                            DesktopEnabled = true,
                            HyphenatedString = "lucky-pet-supplies",
                            Id = 123,
                            IsCustomTracking = false,
                            LogoUrl = "http://image",
                            MerchantBadge = "badge",
                            MobileTrackingNetwork = "",
                            MobileTrackingType = "ExternalBrowser",
                            Name = "Lucky",
                            OfferCount = 0M,
                            RewardType = "Cashback",
                            SpecialTerms = "extended terms",
                            TrackingTimeMax = 90,
                            MinimumSpend = 20
                        }
                    }
                }
            });
        }

        [Test]
        public async Task GetMerchantBundleByFilterAsync_ShouldReturnPremiumOffineDetails_GivenPremiumOfflineMerchant()
        {
            var state = new TestState();
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.Blue,
                HyphenatedString = "lucky-pet-supplies",
                NetworkId = state.InstoreNetworkId,
                TierCommTypeId = 101,
                IsFlatRate = true,
                Commission = 10,
                ClientComm = 100,
                MemberComm = 100
            });
            state.GivenTiers(new MerchantTierView
            {
                TierCommTypeId = 100,
                TierSpecialTerms = "You must spend a minimum of $15.67 to be eligable to this offer"
            });

            var result = await state.MerchantService.GetMerchantBundleByFilterAsync(new MerchantRequestInfoModel
            {
                ClientId = Constants.Clients.CashRewards,
                PremiumClientId = Constants.Clients.Blue
            });

            var merchantBundle = result.Data;
            merchantBundle.Should().BeEquivalentTo(new List<MerchantBundleBasicModel>
            {
                new MerchantBundleBasicModel
                {
                    Offline = new List<CardLinkedBasicMerchantModel>()
                    {
                        new CardLinkedBasicMerchantModel
                        {
                            CommissionType = "percent",
                            HyphenatedString = "lucky-pet-supplies",
                            MobileTrackingNetwork = "",
                            MobileTrackingType = "InAppBrowser",
                            RewardType = "Cashback",
                            Commission = 10m,
                            IsFlatRate = true,
                            CommissionString = "10%",
                            MinimumSpend = 15.67m,
                            Premium = new MerchantBasicPremiumModel
                            {
                                Commission = 10m,
                                CommissionString = "10%",
                                IsFlatRate = true
                            }
                        }
                    },
                }
            });
        }

        [Test]
        public async Task GetMerchantBundleByFilterAsync_ShouldReturnConjoinedPremiumOfflineDetails_GivenOfflineMerchantForStandardAndPremiumClients()
        {
            var state = new TestState();
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "my-cosmetic-clinic",
                NetworkId = state.InstoreNetworkId,
                TierCommTypeId = 101,
                ClientComm = 100,
                MemberComm = 100,
                Commission = 8,
                IsFlatRate = true
            });
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.Blue,
                HyphenatedString = "my-cosmetic-clinic",
                NetworkId = state.InstoreNetworkId,
                TierCommTypeId = 101,
                IsFlatRate = true,
                Commission = 15,
                ClientComm = 100,
                MemberComm = 100
            });
            state.GivenTiers(new MerchantTierView
            {
                TierCommTypeId = 100,
                TierSpecialTerms = "You must spend a minimum of $50"
            });

            var result = await state.MerchantService.GetMerchantBundleByFilterAsync(new MerchantRequestInfoModel
            {
                ClientId = Constants.Clients.CashRewards,
                PremiumClientId = Constants.Clients.Blue
            });

            var merchantBundle = result.Data;
            merchantBundle.Should().BeEquivalentTo(new List<MerchantBundleBasicModel>
            {
                new MerchantBundleBasicModel
                {
                    Offline = new List<CardLinkedBasicMerchantModel>()
                    {
                        new CardLinkedBasicMerchantModel
                        {
                            HyphenatedString = "my-cosmetic-clinic",
                            CommissionType = "percent",
                            MobileTrackingNetwork = "",
                            MobileTrackingType = "InAppBrowser",
                            RewardType = "Cashback",
                            Commission = 8,
                            CommissionString = "8%",
                            IsFlatRate = true,
                            MinimumSpend = 50,
                            Premium = new MerchantBasicPremiumModel
                            {
                                Commission = 15m,
                                IsFlatRate = true,
                                CommissionString = "15%"
                            }
                        }
                    }
                }
            });
        }

        [Test]
        public async Task GetMerchantBundleByFilterAsync_ShouldReturnSuppressedMerchants_IfNonPremiumMember()
        {
            var state = new TestState();
            state.SuppressedMerchants.ForEach(state.GivenMerchant);

            var result = await state.MerchantService.GetMerchantBundleByFilterAsync(new MerchantRequestInfoModel
            {
                ClientId = Constants.Clients.CashRewards
            });

            var merchantBundle = result.Data;

            var expectedMerchantIds = new int[] { 123, 124 };

            var resultMerchantIds = merchantBundle.SelectMany(merchantBundle => FlatMapMerchantBundleBasicModel(merchantBundle)).Select(merchant => merchant.Id);

            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }

        [Test]
        public async Task GetMerchantBundleByFilterAsync_ShouldNotReturnSuppressedMerchants_IfPremiumMember()
        {
            var state = new TestState();
            state.SuppressedMerchants.ForEach(state.GivenMerchant);

            var result = await state.MerchantService.GetMerchantBundleByFilterAsync(new MerchantRequestInfoModel
            {
                ClientId = Constants.Clients.CashRewards,
                PremiumClientId = Constants.Clients.Blue
            });

            var merchantBundle = result.Data;

            var expectedMerchantIds = new int[] { 123, 126 };

            var resultMerchantIds = merchantBundle.SelectMany(merchantBundle => FlatMapMerchantBundleBasicModel(merchantBundle)).Select(merchant => merchant.Id);

            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }

        #endregion

        #region GetAllStoresMerchantsByFilterAsync

        [Test]
        public async Task GetAllStoresMerchantsByFilterAsync_ShouldReturnAllStores()
        {
            var state = new TestState();
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "joes-pizza",
                TierCommTypeId = 101,
                IsFlatRate = true,
                Commission = 10.5m,
                ClientComm = 100,
                MemberComm = 100
            });
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "best-n-less",
                TierCommTypeId = 100,
                IsFlatRate = true,
                Commission = 5m,
                ClientComm = 100,
                MemberComm = 100
            });

            var result = await state.MerchantService.GetAllStoresMerchantsByFilterAsync(Constants.Clients.CashRewards, 0, 0, 99, null);

            result.Should().BeEquivalentTo(new PagedList<AllStoresMerchantModel>(2, 2, new List<AllStoresMerchantModel>
            {
                new AllStoresMerchantModel
                {
                    HyphenatedString = "joes-pizza",
                    ClientCommission = 10.5m,
                    CommissionString = "10.5% cashback",
                    Online = true
                },
                new AllStoresMerchantModel
                {
                    HyphenatedString = "best-n-less",
                    CommissionString = "$5 cashback",
                    ClientCommission = 5m,
                    Online = true
                }
            }));
        }

        [Test]
        public async Task GetAllStoresMerchantsByFilterAsync_ShouldReturnPremiumAllStores()
        {
            var state = new TestState();
            state.GivenMerchant(new MerchantFullView
            {
                MerchantId = 1,
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "joes-pizza",
                TierCommTypeId = 100,
                IsFlatRate = true,
                Commission = 10.5m,
                ClientComm = 100,
                MemberComm = 100
            });
            state.GivenMerchant(new MerchantFullView
            {
                MerchantId = 1,
                ClientId = Constants.Clients.Blue,
                HyphenatedString = "joes-pizza",
                TierCommTypeId = 100,
                IsFlatRate = true,
                Commission = 10.5m,
                ClientComm = 100,
                MemberComm = 100
            });
            state.GivenMerchant(new MerchantFullView
            {
                MerchantId = 2,
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "best-n-less",
                TierCommTypeId = 100,
                IsFlatRate = true,
                Commission = 5m,
                ClientComm = 100,
                MemberComm = 100
            });
            state.GivenMerchant(new MerchantFullView
            {
                MerchantId = 2,
                ClientId = Constants.Clients.Blue,
                HyphenatedString = "best-n-less",
                TierCommTypeId = 100,
                IsFlatRate = true,
                Commission = 5m,
                ClientComm = 100,
                MemberComm = 100
            });

            var result = await state.MerchantService.GetAllStoresMerchantsByFilterAsync(Constants.Clients.CashRewards, 0, 0, 99, Constants.Clients.Blue);

            result.Should().BeEquivalentTo(new PagedList<AllStoresMerchantModel>(2, 2, new List<AllStoresMerchantModel>
            {
                new AllStoresMerchantModel
                {
                    Id = 1,
                    HyphenatedString = "joes-pizza",
                    CommissionString = "$10.50 cashback",
                    ClientCommission = 10.5m,
                    Online = true,
                    Premium = new PremiumDto
                    {
                        Commission = 10.5m,
                        ClientCommissionString = "$10.50",
                        IsFlatRate = true
                    }
                },
                new AllStoresMerchantModel
                {
                    Id = 2,
                    HyphenatedString = "best-n-less",
                    CommissionString = "$5 cashback",
                    ClientCommission = 5m,
                    Online = true,
                    Premium = new PremiumDto
                    {
                        Commission = 5m,
                        ClientCommissionString = "$5",
                        IsFlatRate = true
                    }
                }
            }));
        }

        [Test]
        public async Task GetAllStoresMerchantsByFilterAsync_ShouldReturnConjoinedStandardAndPremiumAllStores()
        {
            var state = new TestState();
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "joes-pizza",
                TierCommTypeId = 101,
                IsFlatRate = true,
                Commission = 10.5m,
                ClientComm = 100,
                MemberComm = 100
            });
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.Blue,
                HyphenatedString = "joes-pizza",
                TierCommTypeId = 101,
                IsFlatRate = true,
                Commission = 15.5m,
                ClientComm = 100,
                MemberComm = 100
            });

            var result = await state.MerchantService.GetAllStoresMerchantsByFilterAsync(Constants.Clients.CashRewards, 0, 0, 99, Constants.Clients.Blue);

            result.Should().BeEquivalentTo(new PagedList<AllStoresMerchantModel>(1, 1, new List<AllStoresMerchantModel>
            {
                new AllStoresMerchantModel
                {
                    HyphenatedString = "joes-pizza",
                    CommissionString = "10.5% cashback",
                    ClientCommission = 10.5m,
                    Online = true,
                    Premium = new PremiumDto
                    {
                        Commission = 15.5m,
                        ClientCommissionString = "15.5%",
                        IsFlatRate = true
                    }
                }
            }));
        }

        [Test]
        public async Task GetAllStoresMerchantsByFilterAsync_ShouldReturnNonSuppressedMerchants_IfNonPremiumClient()
        {
            var state = new TestState();
            state.SuppressedMerchants.Where(merchant => merchant.ClientId == Constants.Clients.CashRewards)
                    .ToList().ForEach(state.GivenMerchant);

            var result = await state.MerchantService.GetAllStoresMerchantsByFilterAsync(Constants.Clients.CashRewards, 0, 0, 99, null);

            var expectedMerchantIds = new int[] { 123, 124 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);

            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }

        [Test]
        public async Task GetAllStoresMerchantsByFilterAsync_ShouldNotReturnSuppressedMerchants_IfPremiumClient()
        {
            var state = new TestState();
            state.SuppressedMerchants.ForEach(state.GivenMerchant);

            var result = await state.MerchantService.GetAllStoresMerchantsByFilterAsync(Constants.Clients.CashRewards, 0, 0, 99, Constants.Clients.Blue);

            var expectedMerchantIds = new int[] { 123, 126 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);

            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }

        [Test]
        public async Task GetAllStoresMerchantsByFilterAsync_ShouldReturnStoresOnlyHaveCashbackForCashrewardClient()
        {
            var state = new TestState();
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "joes-pizza",
                TierCommTypeId = 101,
                IsFlatRate = true,
                Commission = 10.5m,
                ClientComm = 100,
                MemberComm = 100,
            });
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "best-n-less",
                TierCommTypeId = 100,
                IsFlatRate = true,
                Commission = 0m,
                ClientComm = 0,
                MemberComm = 0
            });

            var expectedMerchantIds = new int[] { 123, 126 };
            var clientIds = new List<int> { Constants.Clients.CashRewards };
            var merchants = new List<string> { "joes-pizza"};
            
            var result = await state.MerchantService.GetAllStoresMerchantsByFilterAsync(Constants.Clients.CashRewards, 0, 0, 99, null);
            state.Repository.Verify(p => p.QueryAsync<MerchantFullView>(It.IsAny<string>(),
                It.Is<object>(param =>
                ((IList<int>)param.GetType().GetProperty("ClientIds").GetValue(param)).All(t => clientIds.Contains(t)) &&
                ((IList<string>)param.GetType().GetProperty("UniqueMerchanthyphenatedStrings").GetValue(param)).All(t => merchants.Contains(t))
                 )
                ), Times.Once);

        }

        #region IsPaused merchant feature flag CPS-68 CPS-1379
        [Test]
        public async Task GetAllStoresMerchantsByFilterAsync_ShouldReturn_PausedStores_If_PausedMerchantFlag_Is_Off()
        {
            var state = new TestState();
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "joes-pizza",
                TierCommTypeId = 101,
                IsFlatRate = true,
                Commission = 10.5m,
                ClientComm = 100,
                MemberComm = 100,
                MerchantId=1,
                IsPaused=true
            });
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "best-n-less",
                TierCommTypeId = 100,
                IsFlatRate = true,
                Commission = 0m,
                ClientComm = 0,
                MemberComm = 0,
                MerchantId = 2,
                IsPaused = false
            });

            var result = await state.MerchantService.GetAllStoresMerchantsByFilterAsync(Constants.Clients.CashRewards, 0, 0, 99, null);

            var expectedMerchantIds = new int[] { 1, 2 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);

            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }

        [Test]
        public async Task GetAllStoresMerchantsByFilterAsync_ShouldNotReturn_PausedStores_If_PausedMerchantFlag_Is_On()
        {
            var state = new TestState();
            state.featureToggle.Setup(s => s.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(true);

            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "joes-pizza",
                TierCommTypeId = 101,
                IsFlatRate = true,
                Commission = 10.5m,
                ClientComm = 100,
                MemberComm = 100,
                MerchantId = 1,
                IsPaused = true
            });
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "best-n-less",
                TierCommTypeId = 100,
                IsFlatRate = true,
                Commission = 20m,
                ClientComm = 100,
                MemberComm = 100,
                MerchantId = 2,
                IsPaused = false
            });

            var result = await state.MerchantService.GetAllStoresMerchantsByFilterAsync(Constants.Clients.CashRewards, 0, 0, 99, null);

            var expectedMerchantIds = new int[] { 2 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);

            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }

        [Test]
        public async Task GetAllStoresMerchantsByFilterAsync_ShouldReturn_NonPausedStores_If_PausedMerchantFlag_Is_On()
        {
            var state = new TestState();
            state.featureToggle.Setup(s => s.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(true);

            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "joes-pizza",
                TierCommTypeId = 101,
                IsFlatRate = true,
                Commission = 10.5m,
                ClientComm = 100,
                MemberComm = 100,
                MerchantId = 1,
                IsPaused = false
            });
            state.GivenMerchant(new MerchantFullView
            {
                ClientId = Constants.Clients.CashRewards,
                HyphenatedString = "best-n-less",
                TierCommTypeId = 100,
                IsFlatRate = true,
                Commission = 20m,
                ClientComm = 100,
                MemberComm = 100,
                MerchantId = 2,
                IsPaused = false
            });

            var result = await state.MerchantService.GetAllStoresMerchantsByFilterAsync(Constants.Clients.CashRewards, 0, 0, 99, null);

            var expectedMerchantIds = new int[] { 1, 2 };
            var resultMerchantIds = result.Data.Select(merchant => merchant.Id);

            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }

        #endregion

        [Test]
        public async Task GetCmsTrackingMerchantsSearchByFilter_ShouldNotReturnMerchants_IfNotMatchesName()
        {
            var state = new TestState();
            var models = new List<CmsTrackingMerchantSearchResonseModel> {
                new CmsTrackingMerchantSearchResonseModel
                {
                    MerchantId= 1001,
                    MerchantName = "Licquorland",
                    NetworkId = 1,
                    Status = 1
                },
                new CmsTrackingMerchantSearchResonseModel
                {
                    MerchantId= 1002,
                    MerchantName = "Energy Australia",
                    NetworkId = 1,
                    Status = 1
                }
            };

            var expectedMerchantIds = new int[] { 123, 126 };
            state.Repository.Setup(r => r.QueryAsync<CmsTrackingMerchantSearchResonseModel>(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(models);

            var result = await state.MerchantService.GetCmsTrackingMerchantsSearchByFilter(
                new CmsTrackingMerchantSearchFilterRequestModel { 
                    Name = "Licq"
                });
            result.Count.Should().Be(1);
        }

        #endregion

        #region GetMerchantsForStandardAndPremiumClients

        [Test]
        public async Task GetMerchantsForStandardAndPremiumClients_ShouldNotReturnSuppressedMerchants()
        {
            var state = new TestState();
            //var crMerchants = state.SuppressedMerchants.Where(merchant => merchant.ClientId == Constants.Clients.CashRewards);
            state.SuppressedMerchants.ForEach(state.GivenMerchant);
            var merchantIds = state.SuppressedMerchants.Select(merchant => merchant.MerchantId).ToList();

            var result = await state.MerchantService.GetMerchantsForStandardAndPremiumClients(Constants.Clients.CashRewards, Constants.Clients.Blue, merchantIds);

            var expectedMerchantIds = new int[] { 123, 126 };

            var resultMerchantIds = result.Select(merchant => merchant.MerchantId);

            resultMerchantIds.Should().BeEquivalentTo(expectedMerchantIds);
        }
        #endregion

        #region GetPremiumDisabledMerchants

        [Test]
        public void WhenIGetPremiumDisabledMerchants_ShouldOnlyReturnMerchantsWithPremiumDisabledFlagEqualToOne()
        {
            var state = new TestState();
            state.SuppressedMerchants.ForEach(state.GivenMerchant);

            var result = state.MerchantService.GetPremiumDisabledMerchants().Result;

            state.SuppressedMerchants.Count.Should().Be(result.Count());

            state.SuppressedMerchants.ForEach(sm =>
            {
                result.Should().Contain(m => m.MerchantId == sm.MerchantId);
            });
        }
        #endregion
    }
}
