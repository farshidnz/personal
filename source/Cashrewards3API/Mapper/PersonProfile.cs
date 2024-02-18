using AutoMapper;
using Cashrewards3API.Features.Person.Model;

namespace Cashrewards3API.Mapper
{
    using Cashrewards3API.Common;
    using Cashrewards3API.Common.Events;
    using Cashrewards3API.Common.Services.Model;
    using Cashrewards3API.Features.Person.Request.UpdatePerson;
    using System;
    using System.Linq;

    public class PersonProfile : Profile
    {
        public PersonProfile()
        {
            CreateMap<PersonModel, Person>()
               .ForMember(dest => dest.UpdatedDateUTC,
                    opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.DateTimeUTC]));
            CreateMap<Person, PersonModel>()
                .ForMember(dest => dest.CashRewardsMemberId, opts => opts.MapFrom(src => src.Members.FirstOrDefault(member => member.ClientId == Constants.Clients.CashRewards).MemberId))
                .ForMember(dest => dest.MemberNewId, opts => opts.MapFrom(src => src.Members.FirstOrDefault(member => member.ClientId == Constants.Clients.CashRewards).MemberNewId));
                
                
            CreateMap<PersonModel, PersonPremiumStatusHistory>()
                .ForMember(dest => dest.StartedAtUTC,
                    opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.DateTimeUTC]))
                 .ForMember(dest => dest.CreatedDateUTC,
                    opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.DateTimeUTC]))
                .ForMember(dest => dest.EndedAtUTC,
                    opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.DateTimeUTC]))
                .ForMember(dest => dest.UpdatedDateUTC,
                    opts => opts.MapFrom((_, __, ___, context) => context.Items[Constants.Mapper.DateTimeUTC]))
                .ForMember(dest => dest.ClientId, opts => opts.MapFrom(_ => Constants.Clients.Blue));

            CreateMap<UpdatePersonRequest, PersonModel>()
               .ForMember(dest => dest.CognitoId, opts => opts.MapFrom(src => Guid.Parse(src.CognitoId)));

            CreateMap<PersonModel, MemberPremiumUpdateProperty>()
                .ForMember(dest => dest.ExternalMemberId, opts => opts.MapFrom(src => src.MemberNewId));
            CreateMap<PersonModel, MemberPremiumUpdateEvent>()
                .ForMember(dest => dest.ExternalMemberId, opts => opts.MapFrom(src => src.MemberNewId))
                .ForMember(dest => dest.MemberId, opts => opts.MapFrom(src => src.CashRewardsMemberId));
        }
    }
}