using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.ShopGoNetwork.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.ShopGoNetwork.Repository
{
    public interface INetworkRepository
    {
        Task<IEnumerable<Network>> GetNetworks();
    }
    public class NetworkRepository : INetworkRepository
    {
        private readonly IReadOnlyRepository _readOnlyRepository;

        public NetworkRepository(IReadOnlyRepository  readOnlyRepository)
        {
            _readOnlyRepository = readOnlyRepository;
        }

        public async Task<IEnumerable<Network>> GetNetworks()
        {
            var query = @" SELECT  
                                NetworkId, 
                                NetworkName, 
                                TrackingHolder, 
                                DeepLinkHolder, 
                                NetworkKey, 
                                [Status], 
                                TimeZoneId, 
                                GstStatusId 
                            from Network WITH (NOLOCK)";

            return  await _readOnlyRepository.QueryAsync<Network>(query, null);
            
        }
    }
}
