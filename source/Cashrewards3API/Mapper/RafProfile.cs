using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Features.ReferAFriend.Model;

namespace Cashrewards3API.Mapper
{
    public class RafProfile : Profile
    {
        public RafProfile()
        {
            CreateMap<TalkableSignupRequest, TalkableMemberCreateEvent>()
                .ForMember(dest => dest.SiteSlug, opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.SiteSlug]))
                .ForMember(dest => dest.Type, opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.Type]))
                .ForMember(dest => dest.Data, opts => opts.MapFrom(src => src));

            CreateMap<TalkableSignupRequest, TalkableMemberCreateEventData>()
                .ForMember(dest => dest.EventCategory, opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.EventCategory]))
                .ForMember(dest => dest.EventNumber, opts => opts.MapFrom(src => $"referrersignup{src.MemberId}"))
                .ForMember(dest => dest.Uuid, opts => opts.MapFrom(src => src.TalkableUuid))
                .ForMember(dest => dest.CustomerId, opts => opts.MapFrom(src => src.MemberId));
        }
    }
}
