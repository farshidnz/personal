using AutoMapper;
using Cashrewards3API.Features.GiftCard.Model;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Offers;

namespace Cashrewards3API.Mapper
{
    public class GiftCardProfile : Profile
    {
        public GiftCardProfile()
        {
            CreateMap<GiftCard, GiftCardDto>();
            CreateMap<Category, GiftCardDto.CategoryDto>();
            CreateMap<Item, GiftCardDto.CategoryDto.ItemDto>();

            CreateMap<MerchantViewModel, GiftCardDto.CategoryDto.ItemDto>()
                .ForMember(dest => dest.MerchantHyphenatedString,
                    opts => opts.MapFrom(src => src.HyphenatedString))
                .ForMember(dest => dest.Name,
                    opts => opts.MapFrom(src => src.MerchantName))
                .ForMember(dest => dest.RateString,
                    opts => opts.MapFrom(src => src.ClientCommissionString));

            CreateMap<PremiumMerchant, GiftCardDto.CategoryDto.ItemDto.PremiumDto>()
                .ForMember(dest => dest.RateString,
                    opts => opts.MapFrom(src => src.ClientCommissionString));

            CreateMap<OfferDto, GiftCardDto.CategoryDto.ItemDto>()
                .ForMember(dest => dest.MerchantHyphenatedString,
                    opts => opts.MapFrom(src => src.HyphenatedString))
                .ForMember(dest => dest.Name,
                    opts => opts.MapFrom(src => src.Title))
                .ForMember(dest => dest.RateString,
                    opts => opts.MapFrom(src => src.ClientCommissionString));

            CreateMap<Premium, GiftCardDto.CategoryDto.ItemDto.PremiumDto>()
                .ForMember(dest => dest.RateString,
                    opts => opts.MapFrom(src => src.ClientCommissionString));
        }
    }
}