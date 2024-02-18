using System;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantTierClientFilter
    {
        public int ClientId { get; set; }
        
        public int MerchantTierId { get; set; }
        
        public DateTime StartDateUtc { get; set; }
        
        public DateTime EndDateUtc { get; set; }
    }
}