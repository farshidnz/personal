using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class OfflineMerchantStore : MerchantStore
    {
        public List<Store> Stores { get; set; }

        public class Store
        {
            public int StoreId { get; set; }

            public string Name { get; set; }

            public string DisplayName { get; set; }

            public string Description { get; set; }

            public string Address { get; set; }

            public string Suburb { get; set; }

            public string PostCode { get; set; }

            public string State { get; set; }

            public string Latitude { get; set; }

            public string Longitude { get; set; }
        }
    }
}
