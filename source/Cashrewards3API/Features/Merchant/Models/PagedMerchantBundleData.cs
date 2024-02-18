using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class PagedMerchantBundleData
    {
        public List<MerchantStoresBundle> Merchants { get; set; }

        public int TotalMerchantNumber { get; set; }
    }
}
