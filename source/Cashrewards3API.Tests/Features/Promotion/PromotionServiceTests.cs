using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Enum;
using Cashrewards3API.Exceptions;
using Cashrewards3API.Features.Category;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Offers;
using Cashrewards3API.Features.Promotion;
using Cashrewards3API.Features.Promotion.Model;
using Cashrewards3API.Mapper;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Cashrewards3API.Common.Constants;
using Cashrewards3API.FeaturesToggle;

namespace Cashrewards3API.Tests.Features.Promotion
{
    [SetCulture("en-Au")]
    public class PromotionServiceTests
    {
        public class TestState
        {
            public PromotionService PromotionService { get; }
            public string PromotionDefinition { get; }

            public Mock<IPromoAppService> PromoAppService { get; }
            public Mock<IMerchantService> MerchantService { get; }
            public Mock<ICategoryService> CategoryService { get; }
            public Mock<IFeatureToggle> FeatureToggle { get; }
            public Mock<IStrapiService> StrapiService { get; }
            public IPausedMerchantFeatureToggle PausedMerchantFeatureToggle { get; }

            public List<MerchantViewModel> MerchantTestData { get; } =
              new List<MerchantViewModel>()
              {
                new MerchantViewModel()
                {
                    MerchantId = 123,
                    HyphenatedString = "Merchant-123",
                    MerchantName = "123",
                    Rate = 1m,
                    Commission = 5m,
                    IsFlatRate = true,
                    TierCommTypeId = 100,
                    ClientId=1000000
                },
                new MerchantViewModel()
                {
                    MerchantId = 456,
                    HyphenatedString = "Merchant-456",
                    MerchantName = "456",
                    Commission = 5m,
                    Rate = 1m,
                    IsFlatRate = true,
                    TierCommTypeId = 100,
                    ClientId=1000000
                }
              };

            public List<OfferDto> OfferTestData { get; } =
                new List<OfferDto>()
                {
                    new OfferDto()
                    {
                        Id = 100,
                        MerchantId = 345,
                        HyphenatedString = "groupon-offer-1",
                        ClientCommissionString = "4% cashback",
                        IsFeatured = false,
                        IsCashbackIncreased = false,
                    },
                    new OfferDto()
                    {
                        Id = 200,
                        MerchantId = 234,
                        HyphenatedString = "Offer-2",
                        ClientCommissionString = "5% cashback",
                        IsFeatured = false,
                        IsCashbackIncreased = false
                    },
                };

            public TestState(bool mockStrapi = true)
            {
                var configs = new Dictionary<string, string>
                {
                    ["Config:PromotionBucketName"] = "cashrewards.dev.hub-pages"
                };

                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(configs)
                    .Build();

                MerchantService = new Mock<IMerchantService>();
                MerchantService.Setup(o => o.GetMerchantsForStandardAndPremiumClients(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<HashSet<int>>()))
                    .ReturnsAsync((int clientId, int premiumClientId, HashSet<int> merchantIds) =>
                        MerchantTestData.Where(m => (m.ClientId == clientId || m.ClientId == premiumClientId) && merchantIds.Contains(m.MerchantId)));
                MerchantService.Setup(o => o.GetMerchantsForStandardClient(It.IsAny<int>(), It.IsAny<HashSet<int>>()))
                    .ReturnsAsync((int clientId, HashSet<int> merchantIds) =>
                        MerchantTestData.Where(m => m.ClientId == clientId && merchantIds.Contains(m.MerchantId)));

                var offerService = new Mock<IOfferService>();
                offerService.Setup(o => o.GetClientOffers(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<HashSet<int>>()))
                    .ReturnsAsync((int clientId, int? premiumClientId, HashSet<int> offerIds) => OfferTestData);

                PromoAppService = new Mock<IPromoAppService>();

                var config = new MapperConfiguration(cfg => cfg.AddMaps("Cashrewards3API"));
                var mapper = config.CreateMapper();

                CategoryService = new Mock<ICategoryService>();

                var awsS3Service = new Mock<IAwsS3Service>();
                awsS3Service.Setup(s => s.ReadAmazonS3Data(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync((string file, string bucket) => TestDataLoader.TryLoad($@".\Features\Promotion\JSON\{file}"));

                StrapiService = new Mock<IStrapiService>();

                StrapiService = new Mock<IStrapiService>();
                StrapiService.Setup(s => s.GetCampaign(It.IsAny<string>()))
                    .ReturnsAsync((string slug) => mockStrapi ? TestDataLoader.TryLoad<StrapiCampaign>($@".\Features\Promotion\JSON\{slug}.strapi.json")
                                                                : null);

                FeatureToggle = new Mock<IFeatureToggle>();
                FeatureToggle.Setup(f => f.IsEnabled(FeatureFlags.IS_MERCHANT_PAUSED)).Returns(true);
                PausedMerchantFeatureToggle = new PausedMerchantFeatureToggle(FeatureToggle.Object);
                PausedMerchantFeatureToggle.IsFeatureEnabled = false;

                var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile(new PromotionProfile()));

                PromotionService = new PromotionService(
                   new PromotionDefinitionService(configuration, mapper, awsS3Service.Object, StrapiService.Object),
                   MerchantService.Object,
                   offerService.Object,
                   mapper,
                   FeatureToggle.Object,
                   PausedMerchantFeatureToggle
               );

                PromotionDefinition = TestDataLoader.Load(@".\Features\Promotion\JSON\mothers-day-s3.json");
            }
        }

        [Test]
        public async Task GetPromotionInfo_ShouldReturnPromotionFromS3_GivenValidPromotionSlug()
        {
            var state = new TestState();

            var promotion = await state.PromotionService.GetPromotionInfo(Clients.CashRewards, null, "mothers-day-s3");

            promotion.Title.Should().Be("Get cashback on Mother's Day deals for every kind of mum!");
            promotion.CampaignSection.Order.Should().Be("top");
        }

        [Test]
        public void GetPromotionInfo_ShouldThrowNotFoundException_GivenNonExistingPromotionSlug()
        {
            var state = new TestState();

            state.Invoking(async s => await s.PromotionService.GetPromotionInfo(Clients.CashRewards, null, ""))
                .Should().Throw<NotFoundException>();
        }

        [Test]
        public async Task GetPromotionInfo_ShouldReturnMerchantItemWithRates_GivenMerchantsRatesAreAvailable()
        {
            var state = new TestState(mockStrapi: false);

            var promotion = await state.PromotionService.GetPromotionInfo(Clients.CashRewards, null, "mothers-day-s3");

            promotion.Categories
                .Single(c => c.CategoryTitle == "Our Top Picks")
                .Items.Any(i => i.RateString == "5% cashback").Should().BeTrue();
        }

        [Test]
        public async Task GetPromotionInfo_ShouldReturnOfferItemWithRates_GivenOfferRatesAreAvailable()
        {
            var state = new TestState(mockStrapi: false);

            var promotion = await state.PromotionService.GetPromotionInfo(Clients.CashRewards, null, "mothers-day-s3");

            promotion.Categories
                .Single(c => c.CategoryTitle == "Gifting")
                .Items.Any(i => i.RateString == "4% cashback").Should().BeTrue();
        }

        [Test]
        public async Task GetPromotionInfo_ShouldReturnPromotionFromStrapi_GivenValidPromotionSlug()
        {
            var state = new TestState();
            state.StrapiService.Setup(s => s.GetCampaign(It.IsAny<string>()))
                .ReturnsAsync((string slug) => TestDataLoader.TryLoad<StrapiCampaign>($@".\Features\Promotion\JSON\{slug}.strapi.json"));

            var promotion = await state.PromotionService.GetPromotionInfo(Clients.CashRewards, null, "mothers-day");

            promotion.Title.Should().Be("Get cashback on Mother's Day deals for every kind of mum!");
        }

        [Test]
        public async Task GetPromotionInfo_ShouldReturn_Valid_Item_Type_For_PromotionFromStrapi_GivenValidPromotionSlug()
        {
            var state = new TestState();
            state.StrapiService.Setup(s => s.GetCampaign(It.IsAny<string>()))
                .ReturnsAsync((string slug) => TestDataLoader.TryLoad<StrapiCampaign>($@".\Features\Promotion\JSON\{slug}.strapi.json"));

            var promotion = await state.PromotionService.GetPromotionInfo(Clients.CashRewards, null, "mothers-day");

            var category = promotion.Categories.First();
            category.Items[0].ItemType.Should().Be((int)PromotionCategoryItemTypeEnum.Merchant);
            category.Items[1].ItemType.Should().Be((int)PromotionCategoryItemTypeEnum.Offer);
            category.Items[2].ItemType.Should().Be((int)PromotionCategoryItemTypeEnum.Offer2Go);
            category.Items[3].ItemType.Should().Be((int)PromotionCategoryItemTypeEnum.Merchant2Go);
        }

        [Test]
        public async Task GetPromotionInfo_ShouldReturnItemId_GivenValidPromotionSlug()
        {
            var state = new TestState();
            state.StrapiService.Setup(s => s.GetCampaign(It.IsAny<string>()))
                .ReturnsAsync((string slug) => TestDataLoader.TryLoad<StrapiCampaign>($@".\Features\Promotion\JSON\{slug}.strapi.json"));

            var promotion = await state.PromotionService.GetPromotionInfo(Clients.CashRewards, null, "mothers-day");

            var category = promotion.Categories.First();
            category.Items[0].ItemId.Should().Be(123);
            category.Items[1].ItemId.Should().Be(2);
            category.Items[2].ItemId.Should().Be(477100);
            category.Items[3].ItemId.Should().Be(456);
        }

        [Test]
        public async Task GetPromotionInfo_ShouldNotReturnCampaignMerchantItemsWithPremiumDisabledMerchants_GivenMerchantsArePremiumDisabled()
        {
            var state = new TestState();
            state.StrapiService.Setup(s => s.GetCampaign(It.IsAny<string>()))
                .ReturnsAsync((string slug) => TestDataLoader.TryLoad<StrapiCampaign>($@".\Features\Promotion\JSON\{slug}.strapi.json"));
            state.MerchantService.Setup(m => m.GetPremiumDisabledMerchants()).ReturnsAsync(new List<MerchantViewModel>
            {
                new() { MerchantId = 123 }
            });

            var promotion = await state.PromotionService.GetPromotionInfo(Clients.CashRewards, Clients.Blue, "mothers-day");

            var category = promotion.Categories.First();
            category.Items[0].ItemId.Should().Be(123);
            category.Items[1].ItemId.Should().Be(2);
            category.Items[2].ItemId.Should().Be(477100);
        }

        [Test]
        public async Task GetPromotionInfo_ShouldNotReturnCampaignOfferItemsWithPremiumDisabledMerchants_GivenMerchantsArePremiumDisabled()
        {
            var state = new TestState();
            state.StrapiService.Setup(s => s.GetCampaign(It.IsAny<string>()))
                .ReturnsAsync((string slug) => TestDataLoader.TryLoad<StrapiCampaign>($@".\Features\Promotion\JSON\{slug}.strapi.json"));
            state.MerchantService.Setup(m => m.GetPremiumDisabledMerchants()).ReturnsAsync(new List<MerchantViewModel>
            {
                new() { MerchantId = 345 }
            });

            var promotion = await state.PromotionService.GetPromotionInfo(Clients.CashRewards, Clients.Blue, "mothers-day");

            var category = promotion.Categories.First();
            category.Items[0].ItemId.Should().Be(123);
            category.Items[1].ItemId.Should().Be(2);
            category.Items[2].ItemId.Should().Be(477100);
        }
    }
}
