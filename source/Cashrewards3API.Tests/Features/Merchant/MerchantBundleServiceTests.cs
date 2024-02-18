using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Merchant.Repository;
using Cashrewards3API.Features.Offers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Cashrewards3API.Features.Merchant.Models.MerchantStore;

namespace Cashrewards3API.Tests.Features.Merchant
{
    [TestFixture]
    public class MerchantBundleServiceTests
    {
        public int OnlineMerchantId { get; set; } = 1234;
        private int offlineMerchantId = 5678;
        private int cashrewardsClientId = 1000000;
        private int premiumClientId = 1000035;

        private string hyphenatedString = "merchant";

        public MerchantFullView OnlineStore { get; set; }
        private MerchantFullView onlinePremiumStore;
        private MerchantFullView offlineStore;
        private MerchantFullView offlinePremiumStore;

        private IEnumerable<MerchantTierViewWithBadge> merchantTierViews;

        private Mock<ICacheKey> cacheKey;
        private IConfiguration configuration;
        private Mock<IRedisUtil> redisUtil;
        private Mock<IMerchantRepository> merchantRepository;
        private Mock<IPremiumService> premiumService;
        private Mock<IFeatureToggle> _featureToggleMock;

        public MerchantBundleServiceTests() { Setup(); }

        [SetUp]
        public void Setup()
        {
            var configs = new Dictionary<string, string>
            {
                { "config:customtrackingmerchantlist", "1001330" },
                { "config:instorenetworkid", "1000053" },
                { "config:onlinestorenetworkid", "1000059" },
                { "config:mobilespecificnetworkid", "1000061" },
            };

            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configs)
                .Build();

            cacheKey = new Mock<ICacheKey>();
            cacheKey.Setup(c => c.GetMerchantBundleByHyphenatedStringKey(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())).Returns("ClientId:1234:MerchantBundleByHyphenatedString:5678:store");

            redisUtil = new Mock<IRedisUtil>();
            redisUtil
                .Setup(r => r.GetDataAsyncWithEarlyRefresh(It.IsAny<string>(), It.IsAny<Func<Task<MerchantBundleDetailResultModel>>>(), It.IsAny<int>()))
                .Returns((string key, Func<Task<MerchantBundleDetailResultModel>> action, int expiryTime) => action());
            redisUtil
                .Setup(r => r.GetDataAsyncWithEarlyRefresh(It.IsAny<string>(), It.IsAny<Func<Task<MerchantStoresBundle>>>(), It.IsAny<int>()))
                .Returns((string key, Func<Task<MerchantStoresBundle>> action, int expiryTime) => action());

            OnlineStore = new MerchantFullView()
            {
                MerchantId = OnlineMerchantId,
                ClientId = cashrewardsClientId,
                HyphenatedString = hyphenatedString,
                Commission = 5,
                ClientComm = 100,
                MemberComm = 100,
                IsFlatRate = false,
                TierCommTypeId = 101
            };

            onlinePremiumStore = new MerchantFullView()
            {
                ClientId = premiumClientId,
                MerchantId = OnlineMerchantId,
                HyphenatedString = hyphenatedString,
                TierCommTypeId = 101,
                ClientComm = 66.6666m,
                Commission = 3m,
                MemberComm = 100m,
                IsFlatRate = false
            };

            offlineStore = new MerchantFullView()
            {
                MerchantId = offlineMerchantId,
                ClientId = cashrewardsClientId,
                HyphenatedString = hyphenatedString,
                NetworkId = 1000053,
                Commission = 4,
                ClientComm = 100,
                MemberComm = 100,
                IsFlatRate = true,
                TierCommTypeId = 101
            };

            offlinePremiumStore = new MerchantFullView()
            {
                MerchantId = offlineMerchantId,
                ClientId = premiumClientId,
                HyphenatedString = hyphenatedString,
                NetworkId = 1000053,
                TierCommTypeId = 101,
                ClientComm = 77m,
                Commission = 10m,
                MemberComm = 100m,
                IsFlatRate = false,
                TrackingTime = "3 to 7 day",
                ApprovalTime = "Up to 14 Days"
            };

            merchantTierViews = new List<MerchantTierViewWithBadge>();

            merchantRepository = new Mock<IMerchantRepository>();

            merchantRepository.Setup(r => r.GetMerchantByClientIdAsync(cashrewardsClientId, OnlineMerchantId)).ReturnsAsync(OnlineStore);
            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(clientIds => clientIds.Contains(cashrewardsClientId)), hyphenatedString))
                .ReturnsAsync(new List<MerchantFullView>() { OnlineStore, offlineStore });

            merchantRepository.Setup(r => r.GetMerchantTierViewsWithBadgeAsync(It.IsAny<IEnumerable<int>>(), OnlineMerchantId)).ReturnsAsync(merchantTierViews);

            SetupSuppressedMerchants(merchantRepository);

            premiumService = new Mock<IPremiumService>();
            premiumService.Setup(s => s.GetPremiumClientId(cashrewardsClientId)).Returns(premiumClientId);

            _featureToggleMock = new Mock<IFeatureToggle>();
        }

        private void SetupSuppressedMerchants(Mock<IMerchantRepository> merchantRepository)
        {
            SuppressedMerchants.ForEach(merchant => merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(clientIds => clientIds.Contains(merchant.ClientId)), merchant.HyphenatedString))
                .ReturnsAsync(new List<MerchantFullView>() { merchant }));
            SuppressedMerchants.ForEach(merchant => merchantRepository.Setup(r => r.GetMerchantByClientIdAsync(merchant.ClientId, merchant.MerchantId)).ReturnsAsync(merchant));
        }

        public List<MerchantFullView> SuppressedMerchants = MerchantTestData.GetSuppressedBundleMerchantsTestData();

        public MerchantBundleService GetMerchantBundleService()
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps("Cashrewards3API"));
            var mapper = config.CreateMapper();
            return new MerchantBundleService(
                configuration,
                new MerchantMappingService(configuration, mapper),
                new Mock<ILogger<MerchantBundleService>>().Object,
                redisUtil.Object,
                cacheKey.Object,
                new CacheConfig(),
                new NetworkExtension(configuration),
                merchantRepository.Object,
                premiumService.Object,
                _featureToggleMock.Object);
        }

        public void MockFeatureToggle(bool on) =>
            _featureToggleMock.Setup(p => p.IsEnabled(It.Is<string>(key => key == FeatureFlags.IS_MERCHANT_PAUSED))).Returns(on);

        [Test]
        public async Task GetMerchantBundleByHyphenatedString_ShouldCallCacheKey()
        {
            var service = GetMerchantBundleService();

            var result = await service.GetMerchantStoresBundleByHyphenatedString(1234, 3456, "merchant", false);

            cacheKey.Verify(c => c.GetMerchantBundleByHyphenatedStringKey(1234, "merchant", 3456, false));
        }

        [Test]
        public async Task GetMerchantBundleById_ShouldReturnData()
        {
            var service = GetMerchantBundleService();

            var result = await service.GetMerchantBundleByIdAsync(cashrewardsClientId, OnlineMerchantId);

            result.Should().NotBeNull();
            result.Online.HyphenatedString.Should().Be(hyphenatedString);
            result.Online.Id.Should().Be(OnlineMerchantId);
            result.Offline.First().HyphenatedString.Should().Be(hyphenatedString);
            result.Offline.First().Id.Should().Be(offlineMerchantId);
        }

        [Test]
        public async Task GetMerchantBundleByHyphenatedString_ShouldReturnData()
        {
            var service = GetMerchantBundleService();

            var result = await service.GetMerchantStoresBundleByHyphenatedString(cashrewardsClientId, premiumClientId, hyphenatedString, false);

            result.Should().NotBeNull();
            result.OnlineStore.HyphenatedString.Should().Be(hyphenatedString);
            result.OnlineStore.MerchantId.Should().Be(OnlineMerchantId);
            result.OfflineStores.First().HyphenatedString.Should().Be(hyphenatedString);
            result.OfflineStores.First().MerchantId.Should().Be(offlineMerchantId);
        }

        [Test]
        public async Task GetMerchantBundleByHyphenatedString_ShouldMapPremiumMerchants()
        {
            var service = GetMerchantBundleService();

            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(clientIds => clientIds.Contains(premiumClientId)), hyphenatedString)).ReturnsAsync(
                new List<MerchantFullView>() {
                    OnlineStore,
                    offlineStore,
                    new MerchantFullView()
                    {
                        ClientId = premiumClientId,
                        MerchantId = OnlineMerchantId,
                        HyphenatedString = "merchant",
                        TierCommTypeId = 101,
                        ClientComm = 66.6666m,
                        Commission = 3m,
                        MemberComm = 100m,
                        IsFlatRate = false
                    },
                    new MerchantFullView()
                    {
                        ClientId = premiumClientId,
                        MerchantId = offlineMerchantId,
                        HyphenatedString = "merchant",
                        TierCommTypeId = 100,
                        ClientComm = 50m,
                        Commission = 6m,
                        MemberComm = 100m,
                        IsFlatRate = true
                    }
                }
            );

            var result = await service.GetMerchantStoresBundleByHyphenatedString(cashrewardsClientId, premiumClientId, hyphenatedString, false);

            result.Should().NotBeNull();
            result.OnlineStore.HyphenatedString.Should().Be(hyphenatedString);
            result.OnlineStore.MerchantId.Should().Be(OnlineMerchantId);
            result.OnlineStore.Premium.ClientCommissionSummary.Should().Be("Up to 2%");
            result.OnlineStore.Premium.ClientCommission.Should().Be(2);
            result.OnlineStore.Premium.IsFlatRate.Should().Be(false);
            result.OfflineStores.First().HyphenatedString.Should().Be(hyphenatedString);
            result.OfflineStores.First().MerchantId.Should().Be(offlineMerchantId);
            result.OfflineStores.First().Premium.ClientCommissionSummary.Should().Be("$3");
            result.OfflineStores.First().Premium.ClientCommission.Should().Be(3);
            result.OfflineStores.First().Premium.IsFlatRate.Should().Be(true);
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(false, true)]
        public async Task GetMerchantBundleByHyphenatedString_ShouldMapPremiumTiers_DependingOnWhetherUserIsPremium(bool userIsPremium, bool offline)
        {
            var tierIdWithPremium = 1;
            var tierIdWithoutPremium = 2;

            var merchantId = offline ? offlineMerchantId : OnlineMerchantId;

            var tiers = new List<MerchantTierViewWithBadge>()
            {
                new MerchantTierViewWithBadge()
                {
                    MerchantId = merchantId,
                    MerchantTierId = tierIdWithPremium,
                    ClientComm = 50m,
                    Commission = 2m,
                    MemberComm = 100m,
                    TierCommTypeId = 101,
                    ClientId = cashrewardsClientId,
                    BadgeCode = "New"
                },
                new MerchantTierViewWithBadge()
                {
                    MerchantId = merchantId,
                    MerchantTierId = tierIdWithoutPremium,
                    ClientComm = 25m,
                    Commission = 12m,
                    MemberComm = 100m,
                    TierCommTypeId = 101,
                    ClientId = cashrewardsClientId,
                    BadgeCode = "LimitedTimeOnly"
                }
            };

            var premiumTiers = new List<MerchantTierViewWithBadge>()
            {
                 new MerchantTierViewWithBadge()
                 {
                    MerchantId = merchantId,
                    MerchantTierId = tierIdWithPremium,
                    ClientComm = 50m,
                    Commission = 3m,
                    MemberComm = 100m,
                    TierCommTypeId = 101,
                    ClientId = premiumClientId
                }
            };


            merchantRepository.Setup(r => r.GetMerchantTierViewsWithBadgeAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), merchantId)).ReturnsAsync(tiers.Concat(premiumTiers));
            merchantRepository.Setup(r => r.GetMerchantTierViewsWithBadgeAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Count() == 1), merchantId)).ReturnsAsync(tiers);

            var service = GetMerchantBundleService();

            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), hyphenatedString)).ReturnsAsync(
                new List<MerchantFullView>() {
                    OnlineStore,
                    offlineStore,
                    new MerchantFullView()
                    {
                        ClientId = premiumClientId,
                        MerchantId = merchantId,
                        HyphenatedString = "merchant",
                        TierCommTypeId = 101
                    }
                }
            );

            var result = await service.GetMerchantStoresBundleByHyphenatedString(cashrewardsClientId, userIsPremium ? premiumClientId : null, hyphenatedString, false);

            List<Tier> tiersResult = null;
            if (offline)
            {
                tiersResult = result.OfflineStores[0].Tiers;
            }
            else
            {
                tiersResult = result.OnlineStore.Tiers;
            }

            tiersResult.Count.Should().Be(2);

            var tierWithPremium = tiersResult.Single(t => t.Id == tierIdWithPremium);
            tierWithPremium.ClientCommissionString.Should().Be("1%");
            tierWithPremium.ClientCommission.Should().Be(1);
            tierWithPremium.CommissionType.Should().Be("percent");
            tierWithPremium.BadgeCode.Should().Be("New");

            if (userIsPremium)
            {
                tierWithPremium.Premium.Should().NotBeNull();
                tierWithPremium.Premium.ClientCommissionString.Should().Be("1.5%");
                tierWithPremium.Premium.ClientCommission.Should().Be(1.5m);
                tierWithPremium.Premium.CommissionType.Should().Be("percent");
            }
            else
            {
                tierWithPremium.Premium.Should().BeNull();
            }


            var tierWithoutPremium = tiersResult.Single(t => t.Id == tierIdWithoutPremium);
            tierWithoutPremium.ClientCommissionString.Should().Be("3%");
            tierWithoutPremium.ClientCommission.Should().Be(3);
            tierWithoutPremium.CommissionType.Should().Be("percent");
            tierWithoutPremium.BadgeCode.Should().Be("LimitedTimeOnly");
        }

        [TestCase(true, true, true, true)]
        [TestCase(true, false, true, true)]
        [TestCase(true, false, false, true)]
        [TestCase(false, true, true, true)]
        [TestCase(false, true, false, false)]
        public async Task GetMerchantBundleByHyphenatedString_ShouldMapPremiumOffers(bool hasRegularOffer, bool hasPremiumOffer, bool userIsPremium, bool shouldSeeOffer)
        {
            var offerId = 45567;

            var regularOffers = new List<OfferViewModel>();
            var premiumOffers = new List<OfferViewModel>();

            if (hasRegularOffer)
            {
                regularOffers.Add(
                    new OfferViewModel()
                    {
                        MerchantId = OnlineMerchantId,
                        ClientId = cashrewardsClientId,
                        OfferId = offerId,
                        OfferTitle = "Regular"
                    }
                );
            }

            if (hasPremiumOffer)
            {
                premiumOffers.Add(
                    new OfferViewModel()
                    {
                        MerchantId = OnlineMerchantId,
                        ClientId = premiumClientId,
                        OfferId = offerId,
                        OfferTitle = "Premium"
                    }
                );
            }

            merchantRepository.Setup(r => r.GetMerchantOfferViewsAsync(It.IsAny<IEnumerable<int>>(), OnlineMerchantId)).ReturnsAsync(regularOffers.Concat(premiumOffers));

            var service = GetMerchantBundleService();

            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), hyphenatedString)).ReturnsAsync(
                new List<MerchantFullView>() { OnlineStore, offlineStore, onlinePremiumStore }
            );

            int? userPremiumClientId = userIsPremium ? premiumClientId : null;

            var result = await service.GetMerchantStoresBundleByHyphenatedString(cashrewardsClientId, userPremiumClientId, hyphenatedString, false);

            if (shouldSeeOffer)
            {
                result.OnlineStore.Offers.Count.Should().Be(1);
                var offer = result.OnlineStore.Offers.Single(o => o.Id == offerId);
                if (hasPremiumOffer && !hasRegularOffer)
                {
                    offer.Title.Should().Be("Premium");
                    offer.IsPremium.Should().BeTrue();
                }
                else if (hasPremiumOffer && userIsPremium)
                {
                    result.OnlineStore.Offers.Single().Title.Should().Be("Premium");
                    offer.IsPremium.Should().BeTrue();
                }
                else
                {
                    result.OnlineStore.Offers.Single().Title.Should().Be("Regular");
                    offer.IsPremium.Should().BeFalse();
                }

            }
            else
            {
                result.OnlineStore.Offers.Count.Should().Be(0);
            }

        }

        [Test]
        public async Task GetMerchantBundleByMerchantId_ShouldMapPremiumInMerchants()
        {
            var service = GetMerchantBundleService();

            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), hyphenatedString)).ReturnsAsync(
               new List<MerchantFullView>() { OnlineStore, offlineStore, onlinePremiumStore }
           );

            var result = await service.GetMerchantBundleByIdAsync(cashrewardsClientId, OnlineMerchantId, premiumClientId);

            result.Should().NotBeNull();
            result.Online.HyphenatedString.Should().Be(hyphenatedString);
            result.Online.Id.Should().Be(OnlineMerchantId);
            result.Online.Premium.Commission.Should().Be(2);
            result.Online.Premium.IsFlatRate.Should().BeFalse();

        }

        [Test]
        public async Task GetMerchantBundleByMerchantId_ShouldIncludePremiumMerchants_EvenWhenUserIsNotPremium()
        {
            var service = GetMerchantBundleService();

            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), hyphenatedString)).ReturnsAsync(
               new List<MerchantFullView>() { OnlineStore, offlineStore, onlinePremiumStore, offlinePremiumStore }
           );

            var result = await service.GetMerchantBundleByIdAsync(cashrewardsClientId, OnlineMerchantId, null);

            result.Should().NotBeNull();
            result.Online.Premium.Commission.Should().Be(2);
            result.Online.Premium.IsFlatRate.Should().BeFalse();

            result.Offline[0].Premium.Commission.Should().Be(7.7m);
            result.Offline[0].Premium.IsFlatRate.Should().Be(false);
        }


        [Test]
        public async Task GetMerchantBundleByMerchantId_ShouldIncludeCommissionStringInStandardAndPremiumMerchants()
        {
            var service = GetMerchantBundleService();

            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), hyphenatedString)).ReturnsAsync(
               new List<MerchantFullView>() { OnlineStore, offlineStore, onlinePremiumStore, offlinePremiumStore }
           );

            var result = await service.GetMerchantBundleByIdAsync(cashrewardsClientId, OnlineMerchantId, null);

            result.Should().NotBeNull();
            result.Online.CommissionString.Should().Be("Up to 5%");
            result.Online.Premium.CommissionString.Should().Be("Up to 2%");

            result.Offline[0].CommissionString.Should().Be("4%");
            result.Offline[0].Premium.CommissionString.Should().Be("Up to 7.7%");

        }



        [Test]
        public async Task GetMerchantBundleByMerchantId_ShouldCallCacheKey()
        {
            var service = GetMerchantBundleService();

            var result = await service.GetMerchantBundleByIdAsync(1234, OnlineMerchantId, 1000034, false);

            cacheKey.Verify(c => c.GetMerchantBundleByMerchantIdKey(1234, OnlineMerchantId, 1000034, false, Enum.IncludePremiumEnum.Preview));
        }

        [Test]
        public async Task OfferClientCommissionStringShoulUse2DecimalPlacesAlways()
        {
            var offerId = 45567;

            var offers = new List<OfferViewModel>() {
                new OfferViewModel()
                {
                    MerchantId = OnlineMerchantId,
                    ClientId = cashrewardsClientId,
                    OfferId = offerId,
                    OfferTitle = "Regular",
                    Commission = 2,
                    ClientComm = 100,
                    MemberComm = 100,
                    IsFlatRate = true,
                    TierCommTypeId = 101
                }
            };


            merchantRepository.Setup(r => r.GetMerchantOfferViewsAsync(It.IsAny<IEnumerable<int>>(), OnlineMerchantId)).ReturnsAsync(offers);

            var service = GetMerchantBundleService();

            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), hyphenatedString)).ReturnsAsync(
                new List<MerchantFullView>() { OnlineStore, offlineStore, onlinePremiumStore }
            );

            var result = await service.GetMerchantStoresBundleByHyphenatedString(cashrewardsClientId, premiumClientId, hyphenatedString, false);

            var offer = result.OnlineStore.Offers.Single();

            offer.ClientCommissionString.Should().Be("2%");
        }

        [Test]
        public async Task GetMerchantBundleByMerchantId_ShouldPremiumInOffline()
        {
            var service = GetMerchantBundleService();

            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), hyphenatedString)).ReturnsAsync(
               new List<MerchantFullView>() { OnlineStore, offlineStore, onlinePremiumStore, offlinePremiumStore }
           );

            var result = await service.GetMerchantBundleByIdAsync(cashrewardsClientId, OnlineMerchantId, premiumClientId);

            result.Should().NotBeNull();
            result.Offline[0].Premium.Commission.Should().Be(7.7m);
            result.Offline[0].Premium.IsFlatRate.Should().Be(false);

        }

        [Test]
        public async Task GetMerchantBundleByMerchantId_ShouldHavePremiumInTiers()
        {
            var service = GetMerchantBundleService();

            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), hyphenatedString)).ReturnsAsync(
               new List<MerchantFullView>() { OnlineStore, offlineStore, onlinePremiumStore, offlinePremiumStore }
           );

            merchantRepository.Setup(r => r.GetMerchantTierViewsWithBadgeAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), OnlineMerchantId)).ReturnsAsync(
                new List<MerchantTierViewWithBadge>() {
                    new MerchantTierViewWithBadge()
                    {
                        ClientTierId = 123,
                        MemberComm = 100,
                        ClientComm = 100,
                        Commission = 5,
                        TierCommTypeId = 100,
                        ClientId = cashrewardsClientId
                    },
                    new MerchantTierViewWithBadge()
                    {
                        ClientTierId = 123,
                        MemberComm = 100,
                        ClientComm = 100,
                        Commission = 10,
                        TierCommTypeId = 100,
                        ClientId = premiumClientId
                    }
                }
            );

            merchantRepository.Setup(r => r.GetMerchantTierViewsWithBadgeAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), offlineMerchantId)).ReturnsAsync(
                new List<MerchantTierViewWithBadge>() {
                    new MerchantTierViewWithBadge()
                    {
                        ClientTierId = 456,
                        MemberComm = 100,
                        ClientComm = 100,
                        Commission = 7,
                        TierCommTypeId = 101,
                        ClientId = cashrewardsClientId
                    },
                    new MerchantTierViewWithBadge()
                    {
                        ClientTierId = 456,
                        MemberComm = 100,
                        ClientComm = 100,
                        Commission = 14,
                        TierCommTypeId = 101,
                        ClientId = premiumClientId
                    }
                }
             );

            var result = await service.GetMerchantBundleByIdAsync(cashrewardsClientId, OnlineMerchantId, premiumClientId);

            result.Should().NotBeNull();

            var onlineTier = result.Online.MerchantTiers.Single();
            onlineTier.Commission.Should().Be(5);
            onlineTier.CommissionType.Should().Be("dollar");
            onlineTier.Premium.Commission.Should().Be(10);
            onlineTier.Premium.CommissionType.Should().Be("dollar");

            var offlineTier = result.Offline[0].MerchantTiers.Single();
            offlineTier.Commission.Should().Be(7);
            offlineTier.CommissionType.Should().Be("percent");
            offlineTier.Premium.Commission.Should().Be(14);
            offlineTier.Premium.CommissionType.Should().Be("percent");
        }


        [Test]
        public async Task GetMerchantsByClientIdsAndHyphenatedStringAsync_TiersClientComissionStringsShouldBeFormattedCorrectly()
        {
            var tierIdPercent = 1;
            var tierIdDollar = 2;

            var tiers = new List<MerchantTierViewWithBadge>()
            {
                new MerchantTierViewWithBadge()
                {
                    MerchantId = OnlineMerchantId,
                    MerchantTierId = tierIdDollar,
                    ClientComm = 50m,
                    Commission = 2m,
                    MemberComm = 100m,
                    TierCommTypeId = 100,
                    ClientId = cashrewardsClientId
                },
                new MerchantTierViewWithBadge()
                {
                    MerchantId = OnlineMerchantId,
                    MerchantTierId = tierIdPercent,
                    ClientComm = 25m,
                    Commission = 12m,
                    MemberComm = 100m,
                    TierCommTypeId = 101,
                    ClientId = cashrewardsClientId
                },
                new MerchantTierViewWithBadge()
                 {
                    MerchantId = OnlineMerchantId,
                    MerchantTierId = tierIdPercent,
                    ClientComm = 50m,
                    Commission = 3m,
                    MemberComm = 100m,
                    TierCommTypeId = 101,
                    ClientId = premiumClientId
                },
                new MerchantTierViewWithBadge()
                {
                    MerchantId = OnlineMerchantId,
                    MerchantTierId = tierIdDollar,
                    ClientComm = 50m,
                    Commission = 3m,
                    MemberComm = 100m,
                    TierCommTypeId = 100,
                    ClientId = premiumClientId
                }
            };


            merchantRepository.Setup(r => r.GetMerchantTierViewsWithBadgeAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), OnlineMerchantId)).ReturnsAsync(tiers);

            var service = GetMerchantBundleService();

            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), hyphenatedString)).ReturnsAsync(
                new List<MerchantFullView>() {
                    OnlineStore,
                    offlineStore,
                    new MerchantFullView()
                    {
                        ClientId = premiumClientId,
                        MerchantId = OnlineMerchantId,
                        HyphenatedString = "merchant",
                        TierCommTypeId = 101
                    }
                }
            );

            var result = await service.GetMerchantStoresBundleByHyphenatedString(cashrewardsClientId, premiumClientId, hyphenatedString, false);

            var tiersResult = result.OnlineStore.Tiers;

            tiersResult.Count.Should().Be(2);

            var percentTier = tiersResult.Single(t => t.Id == tierIdPercent);
            percentTier.ClientCommissionString.Should().Be("3%");
            percentTier.CommissionType.Should().Be("percent");


            percentTier.Premium.Should().NotBeNull();
            percentTier.Premium.ClientCommissionString.Should().Be("1.5%");
            percentTier.Premium.CommissionType.Should().Be("percent");

            var dollarTier = tiersResult.Single(t => t.Id == tierIdDollar);
            dollarTier.ClientCommissionString.Should().Be("$1");
            dollarTier.CommissionType.Should().Be("dollar");


            dollarTier.Premium.Should().NotBeNull();
            dollarTier.Premium.ClientCommissionString.Should().Be("$1.50");
            dollarTier.Premium.CommissionType.Should().Be("dollar");

        }

        [Test]
        public async Task GetMerchantBundleById_ShouldReturnSuppressedMerchant_IfNonPremiumClient()
        {
            var service = GetMerchantBundleService();

            var result = await service.GetMerchantBundleByIdAsync(cashrewardsClientId, 123);

            result.Should().NotBeNull();

            result.Online.Id.Should().Be(123);
        }

        [Test]
        public async Task GetMerchantBundleById_ShouldNotReturnSuppressedMerchant_IfPremiumClient()
        {
            var service = GetMerchantBundleService();

            var result = await service.GetMerchantBundleByIdAsync(Constants.Clients.CashRewards, 124, Constants.Clients.Blue);
            result.Should().BeNull();

        }

        [Test]
        public async Task GetMerchantBundleById_ShouldReturnNonSuppressedMerchant_IfPremiumClient()
        {
            var service = GetMerchantBundleService();

            var result = await service.GetMerchantBundleByIdAsync(Constants.Clients.Blue, 126);

            result.Should().NotBeNull();

            result.Online.Id.Should().Be(126);
        }

        [Test]
        public async Task GetMerchantBundleByHyphenatedString_ShouldReturnSuppressedMerchant_IfNonPremium()
        {
            var service = GetMerchantBundleService();

            var merchant = SuppressedMerchants.FirstOrDefault(merchant => merchant.MerchantId == 124 && merchant.IsPremiumDisabled == true);

            var result = await service.GetMerchantStoresBundleByHyphenatedString(cashrewardsClientId, null, merchant.HyphenatedString, false);

            result.Should().NotBeNull();
            result.OnlineStore.HyphenatedString.Should().Be(merchant.HyphenatedString);
            result.OnlineStore.MerchantId.Should().Be(merchant.MerchantId);
        }

        [Test]
        public async Task GetMerchantBundleByHyphenatedString_ShouldReturnNonSuppressedMerchant_IfPremium()
        {
            var hyphenatedString = "adrenaline";
            var merchants = SuppressedMerchants.Where(merchant => string.Equals(merchant.HyphenatedString, hyphenatedString));
           
            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.IsAny<IEnumerable<int>>(), hyphenatedString))
              .ReturnsAsync(merchants);

            var service = GetMerchantBundleService();

            var result = await service.GetMerchantStoresBundleByHyphenatedString(cashrewardsClientId, Constants.Clients.Blue, hyphenatedString, false);

            result.OnlineStore.HyphenatedString.Should().Be(hyphenatedString);
            result.OnlineStore.MerchantId.Should().Be(128);
        }

        [Test]
        public async Task GetMerchantBundleByHyphenatedString_ShouldNotReturnSuppressedMerchant_IfPremium()
        {
            
            var hyphenatedString = "active8me";
            var merchants = SuppressedMerchants.Where(merchant => string.Equals(merchant.HyphenatedString, hyphenatedString));

            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.IsAny<IEnumerable<int>>(), hyphenatedString))
              .ReturnsAsync(merchants);

            var service = GetMerchantBundleService();

            var result = await service.GetMerchantStoresBundleByHyphenatedString(cashrewardsClientId, Constants.Clients.Blue, hyphenatedString, false);

            result.OnlineStore.Should().BeNull();
            result.OfflineStores.Should().BeNullOrEmpty();
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(false, true)]
        public async Task GetMerchantBundleByHyphenatedString_ShouldFilterHiddenTiers(bool userIsPremium, bool offline)
        {
            var tierIdWithPremium = 1;
            var tierIdWithoutPremium = 2;

            var merchantId = offline ? offlineMerchantId : OnlineMerchantId;

            var tiers = new List<MerchantTierViewWithBadge>()
            {
                new MerchantTierViewWithBadge()
                {
                    MerchantId = merchantId,
                    MerchantTierId = tierIdWithPremium,
                    TierCommTypeId = 101,
                    ClientId = cashrewardsClientId,
                    TierTypeId = (int)TierTypeEnum.Hidden
                },
                new MerchantTierViewWithBadge()
                {
                    MerchantId = merchantId,
                    MerchantTierId = tierIdWithoutPremium,
                    TierCommTypeId = 101,
                    ClientId = cashrewardsClientId,
                }
            };

            var premiumTiers = new List<MerchantTierViewWithBadge>()
            {
                 new MerchantTierViewWithBadge()
                 {
                    MerchantId = merchantId,
                    MerchantTierId = tierIdWithPremium,
                    TierCommTypeId = 101,
                    ClientId = premiumClientId,
                    TierTypeId = (int)TierTypeEnum.Hidden
                }
            };


            merchantRepository.Setup(r => r.GetMerchantTierViewsWithBadgeAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), merchantId)).ReturnsAsync(tiers.Concat(premiumTiers));
            merchantRepository.Setup(r => r.GetMerchantTierViewsWithBadgeAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Count() == 1), merchantId)).ReturnsAsync(tiers);

            var service = GetMerchantBundleService();

            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(c => c.Contains(cashrewardsClientId) && c.Contains(premiumClientId)), hyphenatedString)).ReturnsAsync(
                new List<MerchantFullView>() {
                    OnlineStore,
                    offlineStore,
                    new MerchantFullView()
                    {
                        ClientId = premiumClientId,
                        MerchantId = merchantId,
                        HyphenatedString = "merchant",
                        TierCommTypeId = 101
                    }
                }
            );

            var result = await service.GetMerchantStoresBundleByHyphenatedString(cashrewardsClientId, userIsPremium ? premiumClientId : null, hyphenatedString, false);

            List<Tier> tiersResult = null;
            if (offline)
                tiersResult = result.OfflineStores[0].Tiers;
            else
                tiersResult = result.OnlineStore.Tiers;

            tiersResult.Count.Should().Be(1);

            var tierWithPremium = tiersResult.FirstOrDefault(t => t.Id == tierIdWithPremium);
            tierWithPremium.Should().BeNull();
        }


        [Test]
        public async Task GetMerchantBundleByHyphenatedString_FilterWhenMerchantIsPaused()
        {
            var service = GetMerchantBundleService();

            OnlineStore.IsPaused = true;
            merchantRepository.Setup(r => r.GetMerchantsByClientIdsAndHyphenatedStringAsync(It.Is<IEnumerable<int>>(clientIds => clientIds.Contains(premiumClientId)), hyphenatedString)).ReturnsAsync(
                new List<MerchantFullView>() {
                    OnlineStore,
                    offlineStore,

                }
            );

            merchantRepository.Setup(p => p.GetMerchantTierViewsWithBadgeAsync(It.IsAny<IEnumerable<int>>(), It.Is<int>(merchantId => merchantId == OnlineStore.MerchantId))).ReturnsAsync(
                new List<MerchantTierViewWithBadge>()
                {
                    new MerchantTierViewWithBadge
                    {
                        Commission = 10,
                        ClientId =1000000,
                        MemberComm =9,
                        ClientComm =8,
                        TierCommTypeId=100
                    }
                }
                );

            _featureToggleMock.Setup(p => p.IsEnabled(It.Is<string>(key => key == FeatureFlags.IS_MERCHANT_PAUSED))).Returns(true);

            var result = await service.GetMerchantStoresBundleByHyphenatedString(cashrewardsClientId, premiumClientId, hyphenatedString, false);

            result.OnlineStore.ClientCommission.Should().Be(0);
            result.OnlineStore.ClientCommissionSummary.Should().Be(string.Empty);
            result.OnlineStore.Tiers[0].ClientCommission.Should().Be(0);
            result.OnlineStore.Tiers[0].ClientCommissionString.Should().Be(string.Empty);

        }
    }
}
