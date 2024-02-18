using Amazon.SimpleNotificationService;
using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Events;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Features.MemberClick;
using Cashrewards3API.Features.MemberClick.Models;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Mapper;
using Cashrewards3API.Tests.Features.MemberClick.Utils;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MerchantModel = Cashrewards3API.Features.MemberClick.MerchantModel;

namespace Cashrewards3API.Tests.Features.MemberClick
{
    public class MemberClickServiceTests
    {

        private class TestState
        {
            public MemberClickService MemberClickService { get; set; }

            public Mock<IMessage> MessageService { get; private set; }

            public TestState(int memberFirstClickTestData = 0)
            {
                MerchantModelsTestData = TestDataLoader.Load<List<MerchantModelData>>(@"Features\MemberClick\JSON\MerchantViewModelResponse.json");
                MerchantTierViewModelsTestData = TestDataLoader.Load<List<MerchantTierView>>(@"Features\MemberClick\JSON\MerchantTiersResponse.json");
                OfferModelsTestData = TestDataLoader.Load<List<OfferModelData>>(@"Features\MemberClick\JSON\OfferModelResponse.json");
                MemberFirstClickTestData = memberFirstClickTestData;

                var mockRepository = new Mock<IRepository>();
                mockRepository.Setup(s =>
                        s.Query<MerchantModel>(It.IsAny<string>(), It.IsAny<object>(),It.IsAny<int?>()))
                    .ReturnsAsync((string query, object parameters, int? timeout) => MemberClickUtils.GetMerchantModelsTestData(MerchantModelsTestData, query, parameters));

                mockRepository.Setup(s =>
                        s.QueryFirstOrDefault<NetworkModel>(It.IsAny<string>(), It.IsAny<object>()))
                    .ReturnsAsync(NetworkModelsTestData);

                mockRepository.Setup(s =>
                        s.Query<MerchantTierView>(It.IsAny<string>(), It.IsAny<object>(),It.IsAny<int?>()))
                    .ReturnsAsync(MerchantTierViewModelsTestData);

                mockRepository.Setup(s =>
                        s.QueryFirstOrDefault<MerchantTierView>(It.IsAny<string>(), It.IsAny<object>()))
                    .ReturnsAsync(MerchantTierViewTestData);

                mockRepository.Setup(s =>
                        s.QueryFirstOrDefault<int>(It.IsAny<string>(), It.IsAny<object>()))
                    .ReturnsAsync(MemberFirstClickTestData);

                mockRepository.Setup(s =>
                        s.Query<OfferModel>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>()))
                    .ReturnsAsync((string query, object parameters, int? timeout) => MemberClickUtils.GetOfferModelsTestData(OfferModelsTestData, query, parameters));

                var trackingLinkGenerator = new TrackingLinkGenerator();
                var idGeneratorFactory = new IdGeneratorFactory(new List<IIdGeneratorService>()
                {
                    new Base62MemberClientUniqueGeneratorService()
                });
                var iWoolworthsEncryptionProvider = new Mock<IWoolworthsEncryptionProvider>();
                var snsClient = new Mock<IAmazonSimpleNotificationService>();
                var commonConfig = new CommonConfig();
                var mapper = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile<MemberClickProfile>();
                    cfg.AddProfile<MerchantTierProfile>();
                }).CreateMapper();
                
                MessageService = new Mock<IMessage>();

                MemberClickService = new MemberClickService(
                    mockRepository.Object,
                    trackingLinkGenerator,
                    idGeneratorFactory,
                    iWoolworthsEncryptionProvider.Object,
                    snsClient.Object,
                    commonConfig,
                    mapper,
                    MessageService.Object
                );
            }

            public List<MerchantModelData> MerchantModelsTestData { get; }

            public List<MerchantTierView> MerchantTierViewModelsTestData { get; }

            public List<OfferModelData> OfferModelsTestData { get; }

            private NetworkModel NetworkModelsTestData = new NetworkModel()
            {
                NetworkId = 1000001,
                NetworkName = "Network-Name-001"
            };

            private MerchantTierView MerchantTierViewTestData = new MerchantTierView()
            {
                MerchantTierId = 123456,
                TrackingLink = "trackingLink-123456"
            };

            private int MemberFirstClickTestData = 0;
        }

        [Test]
        public async  Task
            GetMemberClickWithTrackingUrlAsync_ShouldReturnTrackingLinkResultModel_GivenTrackingLinkInfoModel()
        {
            var state = new TestState();

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "amazon-australia-M",
                ClientId = Constants.Clients.CashRewards,
                PremiumClientId = Constants.Clients.Blue,
                IncludeTiers = true,
                Member = new MemberContextModel()
            };

           var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);

           result.MerchantId.Should().Be(1010102);
           result.TrackingId.Should().NotBeNullOrEmpty();
           result.Premium.Should().NotBe(null);
           result.Tiers.Should().NotBeEmpty();
        }

        [Test]
        public async Task
            GetMemberClickWithTrackingUrlAsync_ShouldReturnTrackingLinkResultModel_GivenTrackingLinkInfoModelWithoutPremiumClientId()
        {
            var state = new TestState();

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "amazon-australia-M",
                ClientId = Constants.Clients.CashRewards,
                Member = new MemberContextModel()
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);

            result.MerchantId.Should().Be(1010101);
            result.TrackingId.Should().NotBeNullOrEmpty();
            result.Premium.Should().BeNull();
        }

        [Test]
        public async Task
            GetMemberClickWithTrackingUrlAsync_ShouldReturnTrackingLinkResultModelWithTiers_GivenTrackingLinkInfoModel()
        {
            var state = new TestState();

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "amazon-australia-M",
                ClientId = Constants.Clients.CashRewards,
                IncludeTiers = true,
                Member = new MemberContextModel()
            };

        var exprectedMerchantTiers = new List<TrackingLinkResultMerchantTier>
        {
            new TrackingLinkResultMerchantTier
            {
                ClientCommission = 0.0m,
                ClientCommissionString = "0%",
                Name = "All Other Products",
                Premium = null
            },
            new TrackingLinkResultMerchantTier
            {
                ClientCommission = 6.0m,
                ClientCommissionString = "6%",
                Name = "Watches",
                Premium = null
            }
        };

        var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);
        result.Tiers.Should().BeEquivalentTo(exprectedMerchantTiers);
        }

        [Test]
        public async Task
            GetMemberClickWithTrackingUrlAsync_ShouldReturnTrackingLinkResultModelWithNoTiers_GivenIncludeTiersFalse()
        {
            var state = new TestState();

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                CampaignId = 123,
                HyphenatedStringWithType = "amazon-australia-M",
                ClientId = Constants.Clients.CashRewards,
                IncludeTiers = false,
                Member = new MemberContextModel()
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);
            result.Tiers.Should().BeNull();
        }


        [Test]
        public async Task
            GetMemberClickWithTrackingUrlAsync_ShouldReturnTrackingLinkResultModelWithTiers_GivenTrackingLinkInfoFormMerchantTier()
        {
            var state = new TestState();

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "amazon-australia-123456-MT",
                ClientId = Constants.Clients.CashRewards,
                IncludeTiers = true,
                Member = new MemberContextModel()
            };

            var exprectedMerchantTiers = new List<TrackingLinkResultMerchantTier>
            {
                new TrackingLinkResultMerchantTier
                {
                    ClientCommission = 0.0m,
                    ClientCommissionString = "0%",
                    Name = "All Other Products",
                    Premium = null
                },
                new TrackingLinkResultMerchantTier
                {
                    ClientCommission = 6.0m,
                    ClientCommissionString = "6%",
                    Name = "Watches",
                    Premium = null
                }
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);
            result.Tiers.Should().BeEquivalentTo(exprectedMerchantTiers);
        }

        [Test]
        public async Task
            GetMemberClickWithTrackingUrlAsync_ShouldReturnClientCommissionString_GivenForDollarAndPercentage()
        {
            var state = new TestState();
            state.MerchantTierViewModelsTestData[0].TierCommTypeId = (int)Enum.TierCommTypeEnum.Dollar;
            state.MerchantTierViewModelsTestData[0].Commission = 75.5m;
            state.MerchantTierViewModelsTestData[0].MemberComm = 100m;
            state.MerchantTierViewModelsTestData[0].ClientComm = 100m;

            state.MerchantTierViewModelsTestData[1].Commission = 75.5m;
            state.MerchantTierViewModelsTestData[1].MemberComm = 100m;
            state.MerchantTierViewModelsTestData[1].ClientComm = 100m;

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "amazon-australia-123456-MT",
                ClientId = Constants.Clients.CashRewards,
                IncludeTiers = true,
                Member = new MemberContextModel()
            };

            var exprectedMerchantTiers = new List<TrackingLinkResultMerchantTier>
            {
                new TrackingLinkResultMerchantTier
                {
                    ClientCommission = 75.50m,
                    ClientCommissionString = "$75.50",
                    Name = "All Other Products",
                    Premium = null
                },
                new TrackingLinkResultMerchantTier
                {
                    ClientCommission = 75.5m,
                    ClientCommissionString = "75.5%",
                    Name = "Watches",
                    Premium = null
                }
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);
            result.Tiers.Should().BeEquivalentTo(exprectedMerchantTiers);
        }



        [Test]
        public async Task
            GetMemberClickWithTrackingUrlAsync_ShouldReturnTrackingLinkResultModel_GivenTrackingLinkInfoModelWithoutPreviousMemberClick()
        {
            var state = new TestState();

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "amazon-australia-M",
                ClientId = Constants.Clients.CashRewards,
                Member = new MemberContextModel()
                {
                    MemberId = 1
                }
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);

            state.MessageService.Verify(m => m.MemberFirstClickEvent(It.IsAny<MemberFirstClickEvent>()), Times.Once);
        }

        [Test]
        public async Task
        GetMemberClickWithTrackingUrlAsync_ShouldNotReturnTrackingLinkResultModel_GivenTrackingLinkInfoModelWithPreviousMemberClick()
        {
            var state = new TestState(1);
            

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "cashmere-boutique-M",
                ClientId = Constants.Clients.CashRewards,
                Member = new MemberContextModel()
                {
                    MemberId = 1
                }
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);

            state.MessageService.Verify(m => m.MemberFirstClickEvent(It.IsAny<MemberFirstClickEvent>()), Times.Never);
        }

        [Test]
        public async Task GetMemberClickWithTrackingUrlAsync_ShouldReturnMerchantUrlAsTrackingLink_GivenAppToAppMobileTrackingType()
        {
            var state = new TestState();
            var cashmereBoutiqueForCR = state.MerchantModelsTestData.Single(m => m.HyphenatedString == "cashmere-boutique" && m.MerchantId == 1234567);

            cashmereBoutiqueForCR.MobileAppTrackingType = Enum.MobileAppTrackingTypeEnum.AppToApp;

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "cashmere-boutique-M",
                ClientId = Constants.Clients.CashRewards,
                IsMobileApp = true,
                Member = new MemberContextModel()
                {
                    MemberId = 1
                }
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);

            result.MerchantMobileAppTrackingType.Should().Be(Enum.MobileAppTrackingTypeEnum.AppToApp);
            result.TrackingLink.Should().Be("http://www.cashmereboutique.com");
        }

        [Test]
        public async Task GetMemberClickWithTrackingUrlAsync_ShouldReturnTrackingUrlAsTrackingLink_GivenDeeplinkMobileTrackingNetwork()
        {
            var state = new TestState();
            var cashmereBoutiqueForCR = state.MerchantModelsTestData.Single(m => m.HyphenatedString == "cashmere-boutique" && m.MerchantId == 1234567);

            cashmereBoutiqueForCR.MobileAppTrackingType = Enum.MobileAppTrackingTypeEnum.AppToApp;
            cashmereBoutiqueForCR.MobileTrackingNetwork = (int)Enum.MobileTrackingNetworkEnum.Deeplink;

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "cashmere-boutique-M",
                ClientId = Constants.Clients.CashRewards,
                IsMobileApp = true,
                Member = new MemberContextModel()
                {
                    MemberId = 1
                }
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);

            result.MerchantMobileAppTrackingType.Should().Be(Enum.MobileAppTrackingTypeEnum.AppToApp);
            result.TrackingLink.Should().StartWith("https://www.anrdoezrs.net/click-4055150-10471849");
        }

        [Test]
        public async Task GetMemberClickWithTrackingUrlAsync_ShouldReturnPremiumTrackingLink_GivenPremiumTierClientLink()
        {
            var state = new TestState();

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "cashmere-boutique-M",
                ClientId = Constants.Clients.CashRewards,
                PremiumClientId = Constants.Clients.Blue,
                IsMobileApp = false,
                Member = new MemberContextModel()
                {
                    MemberId = 1
                }
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);

            result.TrackingLink.Should().StartWith("https://www.anrdoezrs.net/click-4055150-10471849-ANZ");
        }

        [Test]
        public async Task GetMemberClickWithTrackingUrlAsync_ShouldReturnTrackingLink_GivenOfferLink()
        {
            var state = new TestState();

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "get-it-all-at-amazon-com-444555-O",
                ClientId = Constants.Clients.CashRewards,
                PremiumClientId = null,
                IsMobileApp = false,
                Member = new MemberContextModel()
                {
                    MemberId = 1
                }
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);

            result.TrackingLink.Should().StartWith("https://click.linksynergy.com/fs-bin/click?id=n53ILgFhZLQ&offerid=444555.76&type=3&subid=0");
        }

        [Test]
        public async Task GetMemberClickWithTrackingUrlAsync_ShouldReturnPremiumTrackingLink_GivenPremiumOfferClientLink()
        {
            var state = new TestState();

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "get-it-all-at-amazon-com-444555-O",
                ClientId = Constants.Clients.CashRewards,
                PremiumClientId = Constants.Clients.Blue,
                IsMobileApp = false,
                Member = new MemberContextModel()
                {
                    MemberId = 1
                }
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);

            result.TrackingLink.Should().StartWith("https://click.linksynergy.com/fs-bin/click?id=n53ILgFhZLQ&offerid=444555.76&type=3&subid=0-anz");
        }

        [Test]
        public async Task GetMemberClickWithTrackingUrlAsync_ShouldReturnTheOnlineMerchantId_GivenBothOnlineAndInstoreMerchants()
        {
            var state = new TestState();

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "tasty-red-rooster-471150-O",
                ClientId = Constants.Clients.CashRewards,
                PremiumClientId = null,
                IsMobileApp = false,
                Member = new MemberContextModel()
                {
                    MemberId = 1
                }
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);

            result.MerchantId.Should().Be(1002329);
        }

        [Test]
        public async Task GetMemberClickWithTrackingUrlAsync_ShouldReturnThePremiumCommissionString_GivenMerchantClick_AndGivenBothAnzAndCrClients()
        {
            var state = new TestState();

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "red-rooster-M",
                ClientId = Constants.Clients.CashRewards,
                PremiumClientId = Constants.Clients.Blue,
                IsMobileApp = false,
                Member = new MemberContextModel()
                {
                    MemberId = 1
                }
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);

            result.ClickItemId.Should().Be(1002329);
            result.ClientCommissionString.Should().Be("7.00% cashback");
        }

        [Test]
        public async Task GetMemberClickWithTrackingUrlAsync_ShouldReturnThePremiumCommissionString_GivenOfferClick_AndGivenBothAnzAndCrClients()
        {
            var state = new TestState();

            var trackingLinkInfoModel = new TrackingLinkInfoModel
            {
                HyphenatedStringWithType = "tasty-red-rooster-471150-O",
                ClientId = Constants.Clients.CashRewards,
                PremiumClientId = Constants.Clients.Blue,
                IsMobileApp = false,
                Member = new MemberContextModel()
                {
                    MemberId = 1
                }
            };

            var result = await state.MemberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfoModel);

            result.ClickItemId.Should().Be(471150);
            result.ClientCommissionString.Should().Be("7.00% cashback");
        }
    }
}
