using System;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class PremiumMerchant
    {
        public decimal Commission { get; set; }
        public Nullable<bool> IsFlatRate { get; set; }
        public string ClientCommissionString { get; set; }
    }
}