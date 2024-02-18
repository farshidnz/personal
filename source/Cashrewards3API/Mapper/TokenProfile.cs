using AutoMapper;
using Cashrewards3API.Common.Services.Model;
using Cashrewards3API.Features.Member.Dto;

namespace Cashrewards3API.Mapper
{
    public class TokenProfile : Profile
    {
        public TokenProfile()
        {
            CreateMap<TRAuthTokenDTO, TokenContext>();
        }
    }
}