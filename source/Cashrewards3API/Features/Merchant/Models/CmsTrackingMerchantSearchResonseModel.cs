using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class CmsTrackingMerchantSearchResonseModel 
    {
        public string MerchantName { get; set; }
        public int MerchantId { get; set; }
        public int NetworkId { get; set; }
        public int Status { get; set; }
    }
}
