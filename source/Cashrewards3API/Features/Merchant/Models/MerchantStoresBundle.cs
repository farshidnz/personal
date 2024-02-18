using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantStoresBundle
    {
        public MerchantStore OnlineStore { get; set; }

        public List<OfflineMerchantStore> OfflineStores { get; set; }
    }
}
