using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantBundleBasicModel
    {
        public MerchantBasicModel Online { get; set; }

        public List<CardLinkedBasicMerchantModel> Offline { get; set; }
    }
}
