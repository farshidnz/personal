using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.ShopGoClient.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.ShopGoClient
{
    public class ShopGoClientService : IShopGoClientService
    {
        private readonly IReadOnlyRepository _readOnlyRepository;
        private readonly IMapper _mapper;
        private readonly ICacheKey _cacheKey;
        private readonly IRedisUtil _redisUtil;
        private readonly CacheConfig _cacheConfig;

        public ShopGoClientService(IReadOnlyRepository readOnlyRepository, 
                            IMapper mapper,
                            ICacheKey cacheKey,
                            IRedisUtil redisUtil,
                            CacheConfig cacheConfig)
        {
            _readOnlyRepository = readOnlyRepository;
            _mapper = mapper;
            _cacheKey = cacheKey;
            _redisUtil = redisUtil;
            _cacheConfig = cacheConfig;
        }

        /// <summary>
        /// Returns only active clients where Status is Active
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ShopGoClientResultModel>> GetShopGoClients()
        {
            var clients = (await GetShopGoClientsFromDb()).Where(client => client.Status == (int)StatusEnum.Active);
            return ConvertToResultModel(clients);
        }


        public async Task<IEnumerable<ShopGoClientResultModel>> GetAllShopGoClients()
        {
            var key = _cacheKey.GetClientsKey();
            var clients = await _redisUtil.GetDataAsync(key,
                                    () => GetShopGoClientsFromDb(), _cacheConfig.CategoryDataExpiry);

            return _mapper.Map< ShopGoClientResultModel[]>(clients);
        }

        public IEnumerable<ShopGoClientResultModel> ConvertToResultModel(IEnumerable<ShopGoClientModel> clients)
        {
            return clients.Select(c => _mapper.Map<ShopGoClientResultModel>(c));
        }

        private async Task<IEnumerable<ShopGoClientModel>> GetShopGoClientsFromDb()
        {
            const string shopGoActiveClientsQuery = "SELECT ClientId, ClientName, ClientKey, Status FROM client";
            return await _readOnlyRepository.QueryAsync<ShopGoClientModel>(shopGoActiveClientsQuery);
        }

    }
}
