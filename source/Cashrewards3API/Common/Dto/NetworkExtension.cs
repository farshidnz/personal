using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Dto
{
    public interface INetworkExtension
    {
        bool IsInStoreNetwork(int networkId);
        
        bool IsInCardLinkedNetwork(int networkId);

        bool IsInMobileSpecificNetwork(int networkId);
    }
    public class NetworkExtension : INetworkExtension
    {
        private readonly IConfiguration _configuration;
        private static int _visaInStoreNetworkId = 0;
        private static int _visaOnlineStoreNetworkId = 0;
        private static int _mobileSpecificNetworkId = 0; // 1000061 -- Button network

        private readonly HashSet<int> MobileSpecificNetworkIds;
        private readonly HashSet<int> CardLinkedNetworkIds;
        private readonly HashSet<int> InStoreNetworkIds;


        public NetworkExtension(IConfiguration configuration)
        {
            _configuration = configuration;
            Int32.TryParse(_configuration["Config:InStoreNetworkId"], out _visaInStoreNetworkId);
            Int32.TryParse(_configuration["Config:OnlineStoreNetworkId"], out _visaOnlineStoreNetworkId);
            Int32.TryParse(_configuration["Config:MobileSpecificNetworkId"], out _mobileSpecificNetworkId);

            MobileSpecificNetworkIds = new HashSet<int>
            {
                _mobileSpecificNetworkId
            };

            CardLinkedNetworkIds = new HashSet<int>
            {
                _visaInStoreNetworkId, // 1000053 -- visa in-store network
                _visaOnlineStoreNetworkId, // 1000059 online visa network
            };

            InStoreNetworkIds = new HashSet<int>
            {
                _visaInStoreNetworkId, // 1000053 -- visa in-store network
            };

        }            

        public bool IsInMobileSpecificNetwork(int networkId)
        {
            return MobileSpecificNetworkIds.Contains(networkId);
        }

        public bool IsInCardLinkedNetwork(int networkId)
        {
            return CardLinkedNetworkIds.Contains(networkId);
        }

        public bool IsInStoreNetwork(int networkId)
        {
            return InStoreNetworkIds.Contains(networkId);
        }
    }
}
