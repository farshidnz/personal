using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Offers;
using Cashrewards3API.Features.Promotion.Model;
using System;

namespace Cashrewards3API.Mapper
{
    public class PromotionProfile : Profile
    {
        public PromotionProfile()
        {
            AllowNullCollections = true;
            MapStrapiCampaignToPromotionDefinition();
            MapPromotionDefinitionToPromotionDto();
        }

        private void MapStrapiCampaignToPromotionDefinition()
        {
            CreateMap<StrapiCampaign, PromotionDefinition>()
                .ForMember(dest => dest.LongDescription, opts => opts.MapFrom(src => src.Long_Description))
                .ForMember(dest => dest.BannerImageUrl, opts => opts.MapFrom(src => src.Banner_Image.Desktop.Url))
                .ForMember(dest => dest.BannerImageMobileUrl, opts => opts.MapFrom(src => src.Banner_Image.Mobile.Url))
                .ForMember(dest => dest.BannerImageTabUrl, opts => opts.MapFrom(src => src.Banner_Image.Tablet.Url))
                .ForMember(dest => dest.MetaTitle, opts => opts.MapFrom(src => src.Seo.Meta_Title))
                .ForMember(dest => dest.MetaDescription, opts => opts.MapFrom(src => src.Seo.Meta_Description))
                .ForMember(dest => dest.Categories, opts => opts.MapFrom(src => src.Category))
                .ForMember(dest => dest.CampaignSection, opts => opts.MapFrom(src => src));

            CreateMap<StrapiCategory, PromotionCategoryDefinition>()
                .ForMember(dest => dest.Items, opts => opts.MapFrom(src => src.Item))
                .ForMember(dest => dest.CategoryTitle, opts => opts.MapFrom(src => src.Title));

            CreateMap<StrapiCampaign, CampaignSectionDefinition>()
                .ForMember(dest => dest.LargeHeadImageUrl, opts => opts.MapFrom(src => GetHeadImageOrDefault(src.Campaign_Section, () => src.Campaign_Section.Large_Head_Image)))
                .ForMember(dest => dest.MediumHeadImageUrl, opts => opts.MapFrom(src => GetHeadImageOrDefault(src.Campaign_Section, () => src.Campaign_Section.Medium_Head_Image)))
                .ForMember(dest => dest.SmallHeadImageUrl, opts => opts.MapFrom(src => GetHeadImageOrDefault(src.Campaign_Section, () => src.Campaign_Section.Small_Head_Image)))
                .ForMember(dest => dest.HeadSubtitle, opts => opts.MapFrom(src => src.Campaign_Section != null ? src.Campaign_Section.Head_Subtitle : null))
                .ForMember(dest => dest.Order, opts => opts.MapFrom(src => src.Campaign_Section != null ? src.Campaign_Section.Order : null));

            CreateMap<StrapiCampaigns, CampaignDefinition>()
                .ForMember(dest => dest.CampaignImageUrl, opts => opts.MapFrom(src => src.Campaign_Image.Url));

            CreateMap<StrapiOffers, OfferDefinition>()
                .ForMember(dest => dest.OfferId, opts => opts.MapFrom(src => src.Offer_Id))
                .ForMember(dest => dest.OfferTitle, opts => opts.MapFrom(src => src.Offer_Title))
                .ForMember(dest => dest.OfferImageUrl, opts => opts.MapFrom(src => src.Offer_Image.Url))
                .ForMember(dest => dest.Price, opts => opts.MapFrom(src => src.Price));

            CreateMap<StrapiCategoryItem, PromotionCategoryItemDefinition>()
                .ForMember(dest => dest.ItemId, opts => opts.MapFrom(src => src.Merchant_Id.HasValue && src.Merchant_Id.Value != 0 ? src.Merchant_Id : src.Offer_Id))
               .ForMember(dest => dest.ItemType, opts => opts.MapFrom(src => GetItemType(src.Merchant_Id, src.Redirect_Go_Page)))
                .ForMember(dest => dest.PastRate, opts => opts.MapFrom(src => src.Past_Rate))
                .ForMember(dest => dest.BackgroundUrl, opts => opts.MapFrom(src => src.Background_Image.Url));

             }

        private void MapPromotionDefinitionToPromotionDto()
        {
            CreateMap<PromotionDefinition, PromotionDto>();
            CreateMap<PromotionCategoryDefinition, PromotionCategoryInfo>();
            CreateMap<PromotionCategoryItemDefinition, PromotionCategoryItemData>();

            CreateMap<PromotionMerchantDefinition, PromotionMerchantData>();
            CreateMap<MerchantViewModel, PromotionMerchantData>()
                .ForMember(dest => dest.MerchantUrlString, opts => opts.MapFrom(src => src.HyphenatedString))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.DescriptionShort))
                .ForMember(dest => dest.SmallImageUrl, opts => opts.MapFrom(src => src.SmallImageUrlSecure))
                .ForMember(dest => dest.RegularImageUrl, opts => opts.MapFrom(src => src.RegularImageUrlSecure))
                .ForMember(dest => dest.RateString, opts => opts.MapFrom(src => src.ClientCommissionString))
                .ForMember(dest => dest.LogoUrl, opts => opts.MapFrom(src => src.RegularImageUrl));

            CreateMap<PromotionMerchantDefinition, PromotionMerchantInfo>()
                .ForMember(dest => dest.Merchant, opts => opts.MapFrom(src => new PromotionMerchantData { MerchantId = src.MerchantId }));

            CreateMap<MerchantViewModel, PromotionCategoryItemData>()
                .ForMember(dest => dest.HyphenatedString, opts => opts.MapFrom(src => src.HyphenatedString))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.MerchantName))
                .ForMember(dest => dest.RateString, opts => opts.MapFrom(src => src.ClientCommissionString))
                .ForMember(dest => dest.LogoUrl, opts => opts.MapFrom(src => src.RegularImageUrl));

            CreateMap<Features.Merchant.Models.PremiumMerchant, Features.Promotion.Model.Premium>()
                .ForMember(dest => dest.RateString, opts => opts.MapFrom(src => src.ClientCommissionString));

            CreateMap<OfferDto, PromotionCategoryItemData>()
                .ForMember(dest => dest.HyphenatedString, opts => opts.MapFrom(src => src.HyphenatedString))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Title))
                .ForMember(dest => dest.RateString, opts => opts.MapFrom(src => src.ClientCommissionString))
                .ForMember(dest => dest.LogoUrl, opts => opts.MapFrom(src => src.RegularImageUrl))
                .ForMember(dest => dest.Premium, opts => opts.MapFrom(src => src.Premium));

            CreateMap<Features.Offers.Premium, Features.Promotion.Model.Premium>()
                .ForMember(dest => dest.RateString, opts => opts.MapFrom(src => src.ClientCommissionString));

            CreateMap<CampaignSectionDefinition, CampaignSectionInfo>()
                .ForMember(dest => dest.Campaigns, opts => opts.Ignore());

            CreateMap<CampaignSectionDefinition, HeadImageUrls>()
                .ForMember(dest => dest.Large, opts => opts.MapFrom(src => src.LargeHeadImageUrl))
                .ForMember(dest => dest.Medium, opts => opts.MapFrom(src => src.MediumHeadImageUrl))
                .ForMember(dest => dest.Small, opts => opts.MapFrom(src => src.SmallHeadImageUrl));

            CreateMap<CampaignDefinition, CampaignInfo>()
                .ForMember(dest => dest.Offers, opts => opts.Ignore());

            CreateMap<OfferDefinition, OfferInfo>()
                .ForMember(dest => dest.OfferHyphenatedString,
                    opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.HyphenatedString]))
                .ForMember(dest => dest.MerchantId,
                    opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.MerchantId]));

        }

        private static PromotionCategoryItemTypeEnum GetItemType(int? merchantId, bool? redirectGoPage)
        {
            if (merchantId.HasValue && merchantId.Value != 0)
            {
                return redirectGoPage.HasValue && redirectGoPage.Value ? PromotionCategoryItemTypeEnum.Merchant2Go : PromotionCategoryItemTypeEnum.Merchant;
            }
            else
            {
                return redirectGoPage.HasValue && redirectGoPage.Value ? PromotionCategoryItemTypeEnum.Offer2Go : PromotionCategoryItemTypeEnum.Offer;
            }
        }

        private static string GetHeadImageOrDefault(StrapiCampaignSection strapiCampaignSection, Func<StrapiMedia> mediaSelector) =>
            (strapiCampaignSection != null && mediaSelector() != null) ? mediaSelector().Url : StrapiCampaignSection.DefaultHeadImage;
    } 
}
