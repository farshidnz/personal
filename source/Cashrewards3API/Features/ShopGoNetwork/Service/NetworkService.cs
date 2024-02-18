using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.ShopGoNetwork.Model;
using Cashrewards3API.Features.ShopGoNetwork.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.ShopGoNetwork.Service
{
    public interface INetworkService
    {
        Task<PagedList<NetworkDto>> GetNetworks(NetworkFilterRequest filter);
    }

    public class NetworkService : INetworkService
    {
        private readonly IMapper _mapper;
        private readonly ICacheKey _cacheKey;
        private readonly IRedisUtil _redisUtil;
        private readonly CacheConfig _cacheConfig;
        private readonly INetworkRepository _networkRepository;

        public NetworkService(
            IMapper mapper, ICacheKey cacheKey, IRedisUtil redisUtil, CacheConfig cacheConfig, INetworkRepository networkRepository)
        {
            _mapper = mapper;
            _cacheKey = cacheKey;
            _redisUtil = redisUtil;
            _cacheConfig = cacheConfig;
            _networkRepository = networkRepository;
        }

        public async Task<PagedList<NetworkDto>> GetNetworks(NetworkFilterRequest filter)
        {
            string key = _cacheKey.GetNetworkKey();
            var networks = await _redisUtil.GetDataAsync(key,
                                    () => GetNetworksFromDb(filter), _cacheConfig.CategoryDataExpiry);

            var totalCount = networks.Count();
            networks = networks.Skip(filter.Skip).Take(filter.Limit);
            return new PagedList<NetworkDto>(totalCount, networks.Count(), networks.ToList());
        }

        private async Task<IEnumerable<NetworkDto>> GetNetworksFromDb(NetworkFilterRequest filter)
        {
            var networks = await _networkRepository.GetNetworks();
            return _mapper.Map<NetworkDto[]>(networks);
        }


    }
}
