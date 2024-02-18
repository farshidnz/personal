using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.MemberClick.Models
{
    public class TrackingLinkResultMerchantTier
    {
        public string Name { get; set; }

        public string ClientCommissionString { get; set; }

        public decimal ClientCommission { get; set; }

        public TrackingLinkResultMerchantTierPremium Premium { get; set; }
    }

    public class TrackingLinkResultMerchantTierPremium 
    {
        public string ClientCommissionString { get; set; }

        public decimal ClientCommission { get; set; }
    }
}
