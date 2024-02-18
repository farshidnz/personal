using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashrewards3API.Enum;

namespace Cashrewards3API.Features.Promotion.Model
{
    public class PromotionDto
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

        public List<PromotionMerchantInfo> MainMerchantList { get; set; }

        public List<PromotionCategoryInfo> Categories { get; set; }

        public CampaignSectionInfo CampaignSection { get; set; }

        public IList<string> Order { get; set; }
    }

    public class PromotionMerchantInfo
    {
        public string PromoImageUrl { get; set; }

        public string PromoText { get; set; }

        public PromotionMerchantData Merchant { get; set; }
    }

    public class PromotionMerchantData
    {
        public int MerchantId { get; set; }

        public string MerchantName { get; set; }

        public string MerchantUrlString { get; set; }

        public string Description { get; set; }

        public string SmallImageUrl { get; set; }

        public string RegularImageUrl { get; set; }

        public int OfferCount { get; set; }

        public string RateString { get; set; }

        public string LogoUrl { get; set; }

        public string BackgroundUrl { get; set; }

        public PremiumMerchant Type { get; set; }

    }

    public class PremiumMerchant
    {
        public decimal Commission { get; set; }
        public Nullable<bool> IsFlatRate { get; set; }
        public string ClientCommissionString { get; set; }
    }

    public class PromotionCategoryInfo
    {
        public string CategoryTitle { get; set; }

        public List<PromotionMerchantData> Merchants { get; set; }

        public List<PromotionCategoryItemData> Items { get; set; }
    }

    public class PromotionCategoryItemData
    {
        public int ItemId { get; set; }

        public string Name { get; set; }

        public string HyphenatedString { get; set; }

        public string RateString { get; set; }

        public string LogoUrl { get; set; }

        public string BackgroundUrl { get; set; }

        public string WasRate { get; set; }

        public string Title { get; set; }

        public int ItemType { get; set; }

        public string MerchantHyphenatedString { get; set; }

        public Premium Premium { get; set; }
    }
    
    public class Premium
    {
        public string RateString { get; set; }
    }

    public class CampaignSectionInfo
    {
        public HeadImageUrls HeadImageUrls { get; set; }

        public string HeadSubtitle { get; set; }

        public List<CampaignInfo> Campaigns { get; set; }

        public string Order { get; set; }
    }

    public class HeadImageUrls
    {
        public string Large { get; set; }

        public string Medium { get; set; }

        public string Small { get; set; }
    }

    public class CampaignInfo
    {
        public string Title { get; set; }

        public string SubTitle { get; set; }

        public string CampaignImageUrl { get; set; }

        public List<OfferInfo> Offers { get; set; }
    }

    public class OfferInfo
    {
        public string OfferTitle { get; set; }

        public string OfferHyphenatedString { get; set; }

        public decimal Price { get; set; }

        public decimal Cashback { get; set; }

        public string OfferImageUrl { get; set; }

        public int MerchantId { get; set; }
    }


}
