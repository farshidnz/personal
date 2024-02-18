using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Merchant.Models;
using System;
using Cashrewards3API.Features.Merchant;

namespace Cashrewards3API.Mapper
{
    public class MerchantTierProfile : Profile
    {
        public MerchantTierProfile()
        {
            CreateMap<MerchantTier, MerchantTierClient>()
               .ForMember(dest => dest.ClientCommission, opts => opts.MapFrom(_ => 100.00m))
               .ForMember(dest => dest.MemberCommission, opts => opts.MapFrom(_ => 100.00m))
               .ForMember(dest => dest.Status, opts => opts.MapFrom(_ => (int)MerchantTierClientStatusTypeEnum.Active))
               .ForMember(dest => dest.ClientId,
                opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.ClientId]));

            CreateMap<CreateInternalMerchantTierRequestModel, MerchantTier>()
                .ForMember(dest => dest.Commission, opts => opts.MapFrom(src => src.PromotionBonusValue))
                .ForMember(dest => dest.TierCommTypeId, opts => opts.MapFrom(src => src.PromotionBonusType))
                .ForMember(dest => dest.TierTypeId, opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.TierTypePromotionId]))
                .ForMember(dest => dest.Status, opts => opts.MapFrom(_ => (int)MerchantTierStatusTypeEnum.Active))
                .ForMember(dest => dest.StartDate, opts => opts.MapFrom(src => TimeZoneInfo.ConvertTime(src.PromotionDateMin, TimeZoneInfo.Utc, Constants.SydneyTimezone)))
                .ForMember(dest => dest.EndDate, opts => opts.MapFrom(src => TimeZoneInfo.ConvertTime(src.PromotionDateMax, TimeZoneInfo.Utc, Constants.SydneyTimezone)))
                .ForMember(dest => dest.IsAdvancedTier, opts => opts.MapFrom(_ => false))
                .ForMember(dest => dest.CurrencyId, opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.CurrencyId]))
                .ForMember(dest => dest.TrackingLink, opts => opts.MapFrom(_ => string.Empty));


            CreateMap<UpdateInternalMerchantTierRequestModel, MerchantTier>()
               .ForMember(dest => dest.Commission, opts => opts.MapFrom(src => src.PromotionBonusValue))
               .ForMember(dest => dest.TierCommTypeId, opts => opts.MapFrom(src => src.PromotionBonusType))
               .ForMember(dest => dest.Status, opts => opts.MapFrom(_ => (int)MerchantTierStatusTypeEnum.Active))
               .ForMember(dest => dest.StartDate, opts => opts.MapFrom(src => TimeZoneInfo.ConvertTime(src.PromotionDateMin, TimeZoneInfo.Utc, Constants.SydneyTimezone)))
               .ForMember(dest => dest.EndDate, opts => opts.MapFrom(src => TimeZoneInfo.ConvertTime(src.PromotionDateMax, TimeZoneInfo.Utc, Constants.SydneyTimezone)));


            CreateMap<MerchantTierView, MerchantTier>();

            CreateMap<MerchantStore.Tier, MerchantTierResultModel>()
                .ForMember(dest => dest.Commission, opts => opts.MapFrom(src => src.ClientCommission))
                .ForMember(dest => dest.CommissionString, opts=>opts.MapFrom(src => src.ClientCommissionString))
                .ForMember(dest => dest.Terms, opts => opts.MapFrom(src => src.TierSpecialTerms));

            CreateMap<PremiumTier, MerchantPremiumTierResultModel>()
                .ForMember(dest => dest.Commission, opts => opts.MapFrom(src => src.ClientCommission))
                .ForMember(dest => dest.CommissionString, opts => opts.MapFrom(src => src.ClientCommissionString));

        }
    }
}