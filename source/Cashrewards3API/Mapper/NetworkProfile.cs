using AutoMapper;
using Cashrewards3API.Features.ShopGoNetwork.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Mapper
{
    public class NetworkProfile : Profile
    {
        public NetworkProfile()
        {
            CreateMap<Network, NetworkDto>()
            .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.NetworkId))
            .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.NetworkName));
        }   
    }
}
