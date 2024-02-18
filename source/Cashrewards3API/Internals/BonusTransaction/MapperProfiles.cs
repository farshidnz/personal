using AutoMapper;
using Cashrewards3API.Internals.BonusTransaction.Models;

namespace Cashrewards3API.Internals.BonusTransaction
{
    public class MapperProfiles : Profile
    {
        public MapperProfiles()
        {
            CreateMap<Models.BonusTransaction, BonusTransactionResultModel>();
        }
    }
}