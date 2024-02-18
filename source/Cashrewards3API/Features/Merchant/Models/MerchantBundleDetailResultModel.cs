using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantBundleDetailResultModel
    {
        public CardLinkedMerchantModel Online { get; set; }

        public List<InStoreMerchantModel> Offline { get; set; }
    }
}
