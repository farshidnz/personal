using AutoMapper;
using Cashrewards3API.Features.Promotion.Model;
using Cashrewards3API.Mapper;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Cashrewards3API.Tests.Mapping
{
    public class PromotionProfileMappingTests
    {
        private class TestState
        {
            public IMapper Mapper { get; }

            public StrapiCampaign StrapiCampaign { get; }

            public PromotionDefinition PromotionDefinition => new PromotionDefinition
            {

            };

            public TestState()
            {
                StrapiCampaign = TestDataLoader.Load<StrapiCampaign>(@".\Mapping\JSON\mothers-day.strapi.json");

                var config = new MapperConfiguration(cfg => cfg.AddProfile<PromotionProfile>());
                Mapper = config.CreateMapper();
            }
        }

        [Test]
        public void Map_ShouldMapToPromotionDefinition_GivenStrapiCampaign()
        {
            var state = new TestState();

            var promotionDefinition = state.Mapper.Map<PromotionDefinition>(state.StrapiCampaign);

            promotionDefinition.Title.Should().Be("Super gifts for Super Mums");
            promotionDefinition.BannerImageUrl.Should().EndWith("/2022_04_27_Mother_s_day_DTP_LP_Banner_1440x260_1f7912d498.png");
            promotionDefinition.BannerImageMobileUrl.Should().EndWith("/2022_00_00_Campaignname_APP_LP_Banner_660x376_0a77178869.png");
            promotionDefinition.BannerImageTabUrl.Should().EndWith("/2022_00_00_Campaignname_MOB_LP_Banner_991x452_db75407b80.png");
            promotionDefinition.MetaTitle.Should().StartWith("meta - title");
            promotionDefinition.MetaDescription.Should().StartWith("meta - desc");
            promotionDefinition.Description.Should().StartWith("When we remember all the super amazing things our mums have done for us");
            promotionDefinition.LongDescription.Should().StartWith("When we remember all the super amazing things our mums have done for us");
            promotionDefinition.Categories[0].CategoryTitle.Should().Be("Top Picks");
            promotionDefinition.Categories[0].Items[0].ItemId.Should().Be(1001527);
            promotionDefinition.Categories[0].Items[0].ItemType.Should().Be(1);
            promotionDefinition.Categories[0].Items[0].BackgroundUrl.Should().EndWith("/bonds_9cbf92a1ed.jpg");
            promotionDefinition.Categories[0].Items[1].ItemId.Should().Be(477100);
            promotionDefinition.Categories[0].Items[1].ItemType.Should().Be(2);
            promotionDefinition.Categories[0].Items[1].BackgroundUrl.Should().EndWith("/apple_02214bfa8e.jpg");
            promotionDefinition.Categories[1].CategoryTitle.Should().Be("Gifting");
            promotionDefinition.Categories[2].CategoryTitle.Should().Be("Beauty");
            promotionDefinition.Categories[3].CategoryTitle.Should().Be("Food & Liquor");
            promotionDefinition.Categories[4].CategoryTitle.Should().Be("Home");
            promotionDefinition.Categories[5].CategoryTitle.Should().Be("Fashion");
            promotionDefinition.CampaignSection.LargeHeadImageUrl.Should().EndWith("/cc_large_head_image.png");
            promotionDefinition.CampaignSection.MediumHeadImageUrl.Should().Be("https://s3-ap-southeast-2.amazonaws.com/cashrewards.prod.hub-pages/Images/spacer-10.png");
            promotionDefinition.CampaignSection.SmallHeadImageUrl.Should().Be("https://s3-ap-southeast-2.amazonaws.com/cashrewards.prod.hub-pages/Images/spacer-10.png");
            promotionDefinition.CampaignSection.HeadSubtitle.Should().Be("campaign sub title");
            promotionDefinition.CampaignSection.Campaigns[0].Title.Should().Be("MothersDay");
            promotionDefinition.CampaignSection.Campaigns[0].SubTitle.Should().Be("mothers");
            promotionDefinition.CampaignSection.Campaigns[0].CampaignImageUrl.Should().Be("/uploads/6_d851a649b1.png");
            promotionDefinition.CampaignSection.Campaigns[0].Offers[0].OfferId.Should().Be(2);
            promotionDefinition.CampaignSection.Campaigns[0].Offers[0].OfferTitle.Should().Be("offer");
            promotionDefinition.CampaignSection.Campaigns[0].Offers[0].Price.Should().Be(20.50m);
            promotionDefinition.CampaignSection.Campaigns[0].Offers[0].Cashback.Should().Be(10.5m);
            promotionDefinition.CampaignSection.Campaigns[0].Offers[0].OfferImageUrl.Should().Be("/uploads/6_d851a649b1.png");
            promotionDefinition.Categories[6].Items[0].PastRate.Should().Be("15");
            promotionDefinition.Categories[6].Items[0].Title.Should().Be("itemTitle");
        }


        [Test]
        public void Map_ShouldMapToPromotionDefinition_GivenStrapiCampaignWithNullSections()
        {
            var state = new TestState();
            state.StrapiCampaign.Banner_Image = null;
            state.StrapiCampaign.Category = null;
            state.StrapiCampaign.Campaign_Section = null;
            state.StrapiCampaign.Seo = null;

            var promotionDefinition = state.Mapper.Map<PromotionDefinition>(state.StrapiCampaign);

            promotionDefinition.MetaTitle.Should().BeNull();
            promotionDefinition.MetaDescription.Should().BeNull();
            promotionDefinition.CampaignSection.LargeHeadImageUrl.Should().Be("https://s3-ap-southeast-2.amazonaws.com/cashrewards.prod.hub-pages/Images/spacer-10.png");
            promotionDefinition.CampaignSection.MediumHeadImageUrl.Should().Be("https://s3-ap-southeast-2.amazonaws.com/cashrewards.prod.hub-pages/Images/spacer-10.png");
            promotionDefinition.CampaignSection.SmallHeadImageUrl.Should().Be("https://s3-ap-southeast-2.amazonaws.com/cashrewards.prod.hub-pages/Images/spacer-10.png");
        }
    }
}
