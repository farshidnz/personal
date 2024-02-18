using AutoMapper;
using Cashrewards3API.Features.Banners.Model;

namespace Cashrewards3API.Mapper
{
    public class BannerProfile : Profile
    {
        public BannerProfile()
        {
            CreateMap<Banner, BannerDto>()
                .ForMember(dest => dest.MobileLink, opts => opts.MapFrom(src => src.MobileAppLink))
                .ForMember(dest => dest.MobileImageUrl, opts => opts.MapFrom(src => src.MobileAppImageUrl))
                .ForMember(dest => dest.MobileBrowserLink, opts => opts.MapFrom(src => src.MobileLink))
                .ForMember(dest => dest.MobileBrowserImageUrl, opts => opts.MapFrom(src => src.MobileImageUrl));
        }
    }
}
