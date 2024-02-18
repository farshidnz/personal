using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Offers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.Merchant
{
    [TestFixture]
    public class MerchantMappingServiceTests
    {
        private MerchantMappingService _merchantMappingService;

        [SetUp] 
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps("Cashrewards3API"));
            var mapper = config.CreateMapper();
            _merchantMappingService = new MerchantMappingService(new Mock<IConfiguration>().Object, mapper) ;
        }
        public IEnumerable<MerchantFullView> premiumMerchants = new List<MerchantFullView>()
        {
          new MerchantFullView(){ MerchantId = 123,ClientId = Constants.Clients.Blue,ClientComm = 90,ClientProgramTypeId = 100,
              TierCommTypeId =101,TierTypeId=118,Commission=10,MemberComm=100,IsFlatRate=false, RewardName = ""},
          new MerchantFullView(){ MerchantId = 1234,ClientId = Constants.Clients.Blue,ClientComm = 90,ClientProgramTypeId = 100,
              TierCommTypeId =101,TierTypeId=118,Commission=10,MemberComm=100,IsFlatRate=false, RewardName = ""}
        };
        List<MerchantStoresBundle> bundles = new List<MerchantStoresBundle>()
            {
                new MerchantStoresBundle()
                {
                    OnlineStore = new MerchantStore()
                    {
                        MerchantId = 123,
                        Name = "a merchant",
                        HyphenatedString = "a-merchant",
                        ClientCommission = 2,
                        IsFlatRate = true,
                        CommissionType = "dollar",
                        LogoUrl = "abc.com"
                    },
                    OfflineStores = new List<OfflineMerchantStore>()
                }
            };


        [Test]
        public void ConvertToAllStoresMerchantModel_ShouldConvertBundleWithPremium()
        {
            var allStores = _merchantMappingService.ConvertToAllStoresMerchantModel(bundles, premiumMerchants);

            var store = allStores.Single();
           
            store.Premium.Should().NotBeNull();
            store.Premium.Commission.Should().Be(9);
            store.Premium.ClientCommissionString.Should().Be("Up to 9%");
        }
        [Test]
        public void ConvertToAllStoresMerchantModel_ShouldConvertBundleWithNullPremiumList()
        {
            var allStores = _merchantMappingService.ConvertToAllStoresMerchantModel(bundles, null);

            var store = allStores.Single();


            store.Premium.Should().BeNull();     
        }

        [Test]
        public void ConvertToAllStoresMerchantModel_ShouldConvertBundleWithNoPremium()
        {
            List<MerchantStoresBundle> bundles = new List<MerchantStoresBundle>()
            {
                new MerchantStoresBundle()
                {
                    OnlineStore = new MerchantStore()
                    {
                        MerchantId = 12356,
                        Name = "a merchant",
                        HyphenatedString = "a-merchant",
                        ClientCommission = 2,
                        IsFlatRate = true,
                        CommissionType = "dollar",
                        LogoUrl = "abc.com"
                    },
                    OfflineStores = new List<OfflineMerchantStore>()
                }
            };
            var allStores = _merchantMappingService.ConvertToAllStoresMerchantModel(bundles, premiumMerchants);

            var store = allStores.Single();
            store.Premium.Should().BeNull();
        }

        [Test]
        public void ConvertToAllStoresMerchantModel_ShouldConvertBundleWithOnlineOnly()
        {
            var bundles = new List<MerchantStoresBundle>()
            {
                new MerchantStoresBundle()
                {
                    OnlineStore = new MerchantStore()
                    {
                        MerchantId = 123,
                        Name = "a merchant",
                        HyphenatedString = "a-merchant",
                        ClientCommission = 2,
                        IsFlatRate = true,
                        CommissionType = "dollar",
                        LogoUrl = "abc.com"
                    },
                    OfflineStores = new List<OfflineMerchantStore>()
                }
            };

            var allStores = _merchantMappingService.ConvertToAllStoresMerchantModel(bundles, premiumMerchants);

            var store = allStores.Single();
            
            store.Id.Should().Be(123);
            store.Name.Should().Be("a merchant");
            store.HyphenatedString.Should().Be("a-merchant");
            store.ClientCommission.Should().Be(2);
            store.CommissionString.Should().Be("$2 cashback");
            store.LogoUrl.Should().Be("abc.com");
            store.Online.Should().BeTrue();
            store.InStore.Should().BeFalse();     
        }

        [Test]
        public void ConvertToAllStoresMerchantModel_ShouldConvertBundleWithInStoreOnly()
        {
            var bundles = new List<MerchantStoresBundle>()
            {
                new MerchantStoresBundle()
                {
                    OnlineStore = null,
                    OfflineStores = new List<OfflineMerchantStore>() {
                        new OfflineMerchantStore()
                        {
                            MerchantId = 123,
                            Name = "a merchant",
                            HyphenatedString = "a-merchant",
                            ClientCommission = 2,
                            IsFlatRate = true,
                            CommissionType = "dollar",
                            LogoUrl = "abc.com",
                            BannerImageUrl = "banner1.png"
                        }
                    }
                }
            };

            var allStores = _merchantMappingService.ConvertToAllStoresMerchantModel(bundles, premiumMerchants);

            var store = allStores.Single();

            store.Id.Should().Be(123);
            store.Name.Should().Be("a merchant");
            store.HyphenatedString.Should().Be("a-merchant");
            store.CommissionString.Should().Be("$2 cashback");
            store.LogoUrl.Should().Be("abc.com");
            store.Online.Should().BeFalse();
            store.InStore.Should().BeTrue();
        }

        [Test]
        public void ConvertToAllStoresMerchantModel_ShouldConvertBundleWithInStoreAndOnline()
        {
            var bundles = new List<MerchantStoresBundle>()
            {
                new MerchantStoresBundle()
                {
                    OnlineStore = new MerchantStore()
                        {
                            MerchantId = 123,
                            Name = "a merchant",
                            HyphenatedString = "a-merchant",
                            ClientCommission = 4,
                            IsFlatRate = true,
                            CommissionType = "percent",
                            LogoUrl = "abc.com",
                            BannerImageUrl = "banner1.png"
                        },
                    OfflineStores = new List<OfflineMerchantStore>() {
                        new OfflineMerchantStore()
                        {
                            MerchantId = 345,
                            Name = "in store merchant",
                            HyphenatedString = "a-merchant",
                            ClientCommission = 4,
                            IsFlatRate = true,
                            CommissionType = "percent",
                            LogoUrl = "def.com",
                            BannerImageUrl = "banner2.png"
                        },
                        new OfflineMerchantStore()
                        {
                            MerchantId = 346,
                            Name = "in store merchant2",
                            HyphenatedString = "a-merchant",
                            ClientCommission = 2,
                            IsFlatRate = true,
                            CommissionType = "percent",
                            LogoUrl = "def.com",
                            BannerImageUrl = "banner3.png"
                        }
                    }
                }
            };

            var allStores = _merchantMappingService.ConvertToAllStoresMerchantModel(bundles, premiumMerchants);

            var store = allStores.Single();

            store.Id.Should().Be(123);
            store.Name.Should().Be("a merchant");
            store.HyphenatedString.Should().Be("a-merchant");
            store.CommissionString.Should().Be("Up to 4% cashback");
            store.LogoUrl.Should().Be("abc.com");
            store.Online.Should().BeTrue();
            store.InStore.Should().BeTrue();
        }

        [Test]
        public void ConvertToAllStoresMerchantModel_ShouldNotThrowIfDataIsIncomplete()
        {
            var bundles = new List<MerchantStoresBundle>()
            {
                new MerchantStoresBundle()
                {
                    OnlineStore = null,
                    OfflineStores = new List<OfflineMerchantStore>()
                },
                new MerchantStoresBundle()
                {
                    OnlineStore = new MerchantStore()
                    {
                        MerchantId = 123,
                        Name = "a merchant",
                        HyphenatedString = "a-merchant",
                        ClientCommission = 2,
                        IsFlatRate = true,
                        CommissionType = "dollar",
                        LogoUrl = "abc.com"
                    },
                    OfflineStores = new List<OfflineMerchantStore>()
                }
            };

            var allStores = _merchantMappingService.ConvertToAllStoresMerchantModel(bundles, premiumMerchants);

            allStores.First().Should().BeNull();
        }

        [TestCase(true, 10, "percent", true, 10, "percent", "10% cashback")]
        [TestCase(true, 10, "percent", true, 12, "percent", "Up to 12% cashback")]
        [TestCase(true, 15, "percent", true, 10, "percent", "Up to 15% cashback")]
        [TestCase(false, 10, "percent", true, 10, "percent", "Up to 10% cashback")]
        [TestCase(true, 10, "percent", false, 10, "percent", "Up to 10% cashback")]
        [TestCase(true, 10, "dollar", true, 10, "dollar", "$10 cashback")]
        [TestCase(true, 10, "dollar", true, 12, "dollar", "Up to $12 cashback")]
        [TestCase(true, 15, "dollar", true, 10, "dollar", "Up to $15 cashback")]
        [TestCase(false, 10, "dollar", true, 10, "dollar", "Up to $10 cashback")]
        [TestCase(true, 10, "dollar", false, 10, "dollar", "Up to $10 cashback")]
        [TestCase(false, 14, "dollar", false, 7, "percent", "Up to 7% cashback")]
        [TestCase(true, 5, "percent", false, 50, "dollar", "Up to 5% cashback")]
        public void ConvertToAllStoresMerchantModel_ShouldHandleCombinationsOfInStoreAndOnlineCommissionsCorrectly(
            bool onlineIsFlatRate, decimal onlineCommission, string onlineCommissionType, 
            bool inStoreIsFlatRate, decimal inStoreCommission, string inStoreCommissionType,
            string expectedCommissionString)
        {
            var bundles = new List<MerchantStoresBundle>()
            {
                new MerchantStoresBundle()
                {
                    OnlineStore = new MerchantStore()
                        {
                            ClientCommission = onlineCommission,
                            IsFlatRate = onlineIsFlatRate,
                            CommissionType = onlineCommissionType,
                        },
                    OfflineStores = new List<OfflineMerchantStore>() {
                        new OfflineMerchantStore()
                        {
                            ClientCommission = inStoreCommission,
                            IsFlatRate = inStoreIsFlatRate,
                            CommissionType = inStoreCommissionType
                        }
                    }
                }
            };

            var allStores = _merchantMappingService.ConvertToAllStoresMerchantModel(bundles, premiumMerchants);

            var store = allStores.Single();

            store.CommissionString.Should().Be(expectedCommissionString);
        }

        [TestCase(true, 11.234, "dollar", true, 1, "dollar", "Up to $11.23 cashback")]
        [TestCase(true, 11.999, "dollar", true, 1, "dollar", "Up to $12 cashback")]
        [TestCase(true, 11.2, "dollar", true, 1, "dollar", "Up to $11.20 cashback")]
        [TestCase(true, 98.76, "percent", true, 1, "percent", "Up to 98.76% cashback")]
        [TestCase(true, 3.6, "percent", true, 1, "percent", "Up to 3.6% cashback")]
        [TestCase(true,55.6666, "percent", true, 1, "percent", "Up to 55.67% cashback")]
        public void ConvertToAllStoresMerchantModel_ShouldHandleDecimalCommissionsCorrectly(
            bool onlineIsFlatRate, decimal onlineCommission, string onlineCommissionType,
            bool inStoreIsFlatRate, decimal inStoreCommission, string inStoreCommissionType,
            string expectedCommissionString)
        {
            var bundles = new List<MerchantStoresBundle>()
            {
                new MerchantStoresBundle()
                {
                    OnlineStore = new MerchantStore()
                        {
                            ClientCommission = onlineCommission,
                            IsFlatRate = onlineIsFlatRate,
                            CommissionType = onlineCommissionType,
                        },
                    OfflineStores = new List<OfflineMerchantStore>() {
                        new OfflineMerchantStore()
                        {
                            ClientCommission = inStoreCommission,
                            IsFlatRate = inStoreIsFlatRate,
                            CommissionType = inStoreCommissionType
                        }
                    }
                }
            };

            var allStores = _merchantMappingService.ConvertToAllStoresMerchantModel(bundles, premiumMerchants);

            var store = allStores.Single();

            store.CommissionString.Should().Be(expectedCommissionString);
        }

        [Test]
        public void ConvertToMerchantTierDto_ShouldMapAllFields()
        {
            var merchantTierView = new MerchantTierViewWithBadge()
            {
                MerchantTierId = 12,
                MerchantId = 12,
                TierDescription = "description",
                Commission = 1.5m,
                ClientComm = 12m,
                MemberComm = 56m,
                TierCommTypeId = 12,
                EndDate = new DateTime(),
                TierImageUrl = "www.something.com",
                TierDescriptionLong = "looong",
                TrackingLink = "something",
                TierSpecialTerms = "special terms",
                TierExclusions = "ex",
                TierReference = "ref",
                Ranking = 2,
                IsFeatured = true
            };

            var result = _merchantMappingService.ConvertToMerchantTierDto(merchantTierView);

            result.MerchantTierId.Should()          .Be(merchantTierView.MerchantTierId);
            result.MerchantId.Should().Be(merchantTierView.MerchantId);
            result.TierDescription.Should().Be(merchantTierView.TierDescription);
            result.Commission.Should().Be(merchantTierView.Commission);
            result.ClientComm.Should().Be(merchantTierView.ClientComm);
            result.MemberComm.Should().Be(merchantTierView.MemberComm);
            result.TierCommTypeId.Should().Be(merchantTierView.TierCommTypeId);
            result.EndDate.Should().Be(merchantTierView.EndDate);
            result.TierImageUrl.Should().Be(merchantTierView.TierImageUrl);
            result.TierDescriptionLong.Should().Be(merchantTierView.TierDescriptionLong);
            result.TrackingLink.Should().Be(merchantTierView.TrackingLink);
            result.TierSpecialTerms.Should().Be(merchantTierView.TierSpecialTerms);
            result.TierExclusions.Should().Be(merchantTierView.TierExclusions);
            result.TierReference.Should().Be(merchantTierView.TierReference);
            result.Ranking.Should().Be(merchantTierView.Ranking);
            result.IsFeatured.Should().Be(merchantTierView.IsFeatured);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ConvertToMerchantStoreTier_ShouldMapAllFields(bool force2DecimalPlaces)
        {
            var merchantTier = new MerchantTier()
            {
                MerchantTierId = 123,
                TierDescription = "test",
                TierCommTypeId = 101,
                EndDate = new DateTime(2020, 10, 10),
                TierSpecialTerms = "t",
                TierExclusions = "ex",
                TierLinks = new List<MerchantTierLink>(),
                TierReference = "R",
                IsFeatured = true,
                Ranking = 10,
                TierDescriptionLong = "loong",
                TierImageUrl = "url",
                TrackingLink = "link",
                ClientComm = 100,
                MemberComm = 50,
                Commission = 3
            };

            var result = _merchantMappingService.ConvertToMerchantStoreTier(merchantTier, force2DecimalPlaces);

            result.Id.Should().Be(merchantTier.MerchantTierId);
            result.Name.Should().Be(merchantTier.TierDescription);
            result.CommissionType.Should().Be("percent");
            result.EndDateTime.Should().Be("2020-10-10T00:00:00");
            result.TierSpecialTerms.Should().Be(merchantTier.TierSpecialTerms);
            result.Exclusions.Should().Be(merchantTier.TierExclusions);
            result.TierReference.Should().Be(merchantTier.TierReference);
            result.IsFeatured.Should().Be(merchantTier.IsFeatured);
            result.Ranking.Should().Be(merchantTier.Ranking);
            result.DescriptionLong.Should().Be(merchantTier.TierDescriptionLong);
            result.TierImageUrl.Should().Be(merchantTier.TierImageUrl);
            result.TrackingLink.Should().Be(merchantTier.TrackingLink);
            if(force2DecimalPlaces)
                result.ClientCommissionString.Should().Be("1.50%");
            else
                result.ClientCommissionString.Should().Be("1.5%");
        }


        [TestCase(true)]
        [TestCase(false)]
        public void ConvertToPremiumMerchant_ShouldMapPremiumMerchant(bool force2DecimalPlaces)
        {
            var merchantFullView = new MerchantFullView()
            {
                TierCommTypeId = 101,
                ClientComm = 66.6666m,
                Commission = 3m,
                MemberComm = 100m,
                IsFlatRate = false,
                NotificationMsg = "notification message",
                ConfirmationMsg = "confirmation message",
                TrackingTime = "tracking time",
                ApprovalTime = "approval time"
            };

            var result = _merchantMappingService.ConvertToPremiumMerchant(merchantFullView, force2DecimalPlaces);

            if (force2DecimalPlaces)
                result.ClientCommissionSummary.Should().Be("Up to 2.00%");
            else
                result.ClientCommissionSummary.Should().Be("Up to 2%");
            result.ClientCommission.Should().Be(2);
            result.IsFlatRate.Should().Be(false);
            result.Notification.Should().Be("notification message");
            result.Confirmation.Should().Be("confirmation message");
            result.TrackingTime.Should().Be("tracking time");
            result.ApprovalTime.Should().Be("approval time");
        }

        [Test]
        public void ConvertToPremiumTier_ShouldMapAllFields()
        {
            var premiumTier = new MerchantTier()
            {
                ClientComm = 50m,
                Commission = 4m,
                MemberComm = 100m,
                TierCommTypeId = 101
            };

            var result = _merchantMappingService.ConvertToPremiumTier(premiumTier);

            result.ClientCommission.Should().Be(2m);
            result.CommissionType.Should().Be("percent");
            result.ClientCommissionString.Should().Be("2%");


        }

        [TestCase(1000064, false)]
        [TestCase(1000062, false)]
        [TestCase(1000000, false)]
        [TestCase(1000063, true)]
        public void ConvertToMerchantStore_ShouldMapIsGiftCardCorrectly(int networkId, bool shouldBeGiftCard)
        {
            var merchantFullView = new MerchantFullView()
            {
                TierCommTypeId = 101,
                ClientComm = 66.6666m,
                Commission = 3m,
                MemberComm = 100m,
                NetworkId = networkId,
                BannerImageUrl = string.Empty,
            };

            var merchantStore = _merchantMappingService.ConvertToMerchantStore(merchantFullView);

            merchantStore.IsGiftCard.Should().Be(shouldBeGiftCard);
        }
    }

}
