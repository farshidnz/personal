using System.Collections.Generic;
using System.Threading.Tasks;
using Cashrewards3API.Features.ShopGoClient.Models;

namespace Cashrewards3API.Features.ShopGoClient
{
    public interface IShopGoClientService
    {
        Task<IEnumerable<ShopGoClientResultModel>> GetShopGoClients();
        Task<IEnumerable<ShopGoClientResultModel>> GetAllShopGoClients();
    }
}