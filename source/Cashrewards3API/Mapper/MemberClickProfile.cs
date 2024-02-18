
using AutoMapper;
using Cashrewards3API.Features.MemberClick.Models;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;

namespace Cashrewards3API.Mapper
{
    public class MemberClickProfile : Profile
    {
        public MemberClickProfile()
        {
            CreateMap<MerchantTier, TrackingLinkResultMerchantTier>()
                .ForMember(dest => dest.Name, 
                    opts => opts.MapFrom(src => src.TierDescription));

            CreateMap<MerchantTier, TrackingLinkResultMerchantTierPremium>();
            CreateMap<TrackingLinkResultMerchantTier, TrackingLinkResultMerchantTierPremium>();
        }
    }
}
