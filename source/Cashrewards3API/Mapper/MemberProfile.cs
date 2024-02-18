using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Member.Model;
using Cashrewards3API.Features.Member.Request;
using System;

namespace Cashrewards3API.Mapper
{
    using Cashrewards3API.Common.Services.Model;
    using Cashrewards3API.Features.Member.Request.SignInMember;

    public class MemberProfile : Profile
    {
        public MemberProfile()
        {
            CreateMap<MemberDto, ClientMemberResultModel>();
            CreateMap<MemberModel, MemberDto>();
            CreateMap<MemberDto, MemberModel>();
            CreateMap<MemberModel, ClientMemberResultModel>();
            CreateMap<MemberModel, MemberContextModel>();

            CreateMap<MemberModel, CognitoMemberModel>()
            .ForMember(dest => dest.Status, opts => opts.MapFrom(_ => true))
            .ForMember(dest => dest.CreatedAt, opts => opts.MapFrom(_ => TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, Constants.SydneyTimezone)))
            .ForMember(dest => dest.MemberNewId, opts => opts.MapFrom(src => src.MemberNewId));

            CreateMap<CreateCognitoMemberRequest, MemberModel>()
            .ForMember(dest => dest.Mobile, opts => opts.MapFrom(src => src.PhoneNumber))
             .ForMember(dest => dest.OriginationSource, opts => opts.MapFrom(src => src.OriginalSource));

            CreateMap<MemberModel, Member>()
                .ForMember(dest => dest.Status, opts => opts.MapFrom(_ => (int)StatusEnum.Active))
                .ForMember(dest => dest.ActivateBy, opts => opts.MapFrom(_ => (TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, Constants.SydneyTimezone)).AddYears(10)))
                .ForMember(dest => dest.DateJoined, opts => opts.MapFrom(_ => TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.Utc, Constants.SydneyTimezone)))
                .ForMember(dest => dest.ReceiveNewsLetter, opts => opts.MapFrom(src => (src.ClientId == Constants.Clients.CashRewards ? true : false)))
                .ForMember(dest => dest.MemberNewId, opts => opts.MapFrom(_ => Guid.NewGuid()));

            CreateMap<MemberPassword, Member>()
            .ForMember(dest => dest.MobileSHA256, opts => opts.MapFrom(src => src.HashedMobile));

            CreateMap<MemberModel, CognitoMemberDto>();

            CreateMap<Member, MemberModel>();


            CreateMap<CognitoMemberModel, MemberModel>()
            .ForMember(dest => dest.CognitoId, opts => opts.MapFrom(src => src.CognitoId))
            .ForMember(dest => dest.PersonId, opts => opts.MapFrom(src => src.PersonId))
            .ForAllOtherMembers(ign => ign.Ignore());

            CreateMap<MemberModel, Person>()
                .ForMember(dest => dest.CreatedDateUTC, opts => opts.MapFrom(__ => DateTime.UtcNow))
                .ForMember(dest => dest.PremiumStatus, opts => opts.MapFrom(__ => (int)PremiumStatusEnum.NotEnrolled));

            CreateMap<MemberInternalModel, SignInMemberInternalResponseMember>()
                .ForMember(dest => dest.AccessCode, opts => opts.MapFrom(src => src.AccessCode ?? string.Empty))
                .ForMember(dest => dest.PhoneNumber, opts => opts.MapFrom(src => (src.Mobile ?? string.Empty).Replace(" ", string.Empty)));

            CreateMap<MemberModel, EmailMemberDto>();
            CreateMap<EmailMemberDto, MemberModel>();
        }
    }
}
