using Cashrewards3API.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class CmsTrackingMerchantSearchFilterRequestModel
    {
        public string Name { get; set; } = "";
        public int? NetworkId { get; set; }
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20;
        
    }
}
