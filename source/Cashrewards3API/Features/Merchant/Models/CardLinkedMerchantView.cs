using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class CardLinkedMerchantView
    {
        public string MerchantHyphenatedString { get; set; }
        public Nullable<bool> IsFeatured { get; set; }
        public Nullable<bool> IsHomePageFeatured { get; set; }
        public Nullable<bool> IsPopular { get; set; }
        public string MerchantName { get; set; }
    }
}
