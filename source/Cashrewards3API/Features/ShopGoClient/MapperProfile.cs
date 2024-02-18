using AutoMapper;
using Cashrewards3API.Features.ShopGoClient.Models;

namespace Cashrewards3API.Features.ShopGoClient
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles()
        {
            CreateMap<ShopGoClientModel, ShopGoClientResultModel>();
        }
    }
}