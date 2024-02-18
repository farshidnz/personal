using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantTierLinkModel
    {
        public int MerchantTierLinkId { get; set; }

        public int MerchantTierId { get; set; }

        public string MerchantTierLinkName { get; set; }

        public string MerchantTierLinkUrl { get; set; }
    }
}
