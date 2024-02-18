using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashrewards3API.Enum;

namespace Cashrewards3API.Features.Promotion.Model
{
    public class PromotionDefinition
    {
        public string Title { get; set; }

        public string BannerImageUrl { get; set; }

        public string BannerImageMobileUrl { get; set; }

        public string BannerImageTabUrl { get; set; }

        public string SecondBannerImageUrl { get; set; }

        public string SecondBannerImageMobileUrl { get; set; }

        public string SecondBannerClickoutUrl { get; set; }

        public string MetaTitle { get; set; }

        public string MetaDescription { get; set; }

        public string Description { get; set; }

        public string LongDescription { get; set; }

        public List<PromotionMerchantDefinition> MainMerchantList { get; set; }

        public List<PromotionCategoryDefinition> Categories { get; set; }

        public CampaignSectionDefinition CampaignSection { get; set; }
    }

    public class PromotionMerchantDefinition
    {
        public string PromoImageUrl { get; set; }

        public string PromoText { get; set; }

        public int MerchantId { get; set; }

        public string BackgroundUrl { get; set; }
    }

    public class PromotionCategoryDefinition
    {
        public string CategoryTitle { get; set; }

        public List<PromotionMerchantDefinition> Merchants { get; set; }

        public List<PromotionCategoryItemDefinition> Items { get; set; }
    }

    public class PromotionCategoryItemDefinition
    {
        public int ItemId { get; set; }

        public string BackgroundUrl { get; set; }

        public PromotionCategoryItemTypeEnum ItemType { get; set; }

        public string PastRate { get; set; }

        public string Title { get; set; }
    }

    public class CampaignSectionDefinition
    {
        public string LargeHeadImageUrl { get; set; }

        public string MediumHeadImageUrl { get; set; }

        public string SmallHeadImageUrl { get; set; }

        public string HeadSubtitle { get; set; }

        public List<CampaignDefinition> Campaigns { get; set; }

        public string Order { get; set; }
    }

    public class CampaignDefinition
    {
        public string Title { get; set; }

        public string SubTitle { get; set; }

        public string CampaignImageUrl { get; set; }

        public List<OfferDefinition> Offers { get; set; }
    }

    public class OfferDefinition
    {
        public int OfferId { get; set; }

        public string OfferTitle { get; set; }

        public decimal Price { get; set; }

        public decimal Cashback { get; set; }

        public string OfferImageUrl { get; set; }
    }

}
