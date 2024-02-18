using System;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantTierClient
    {
        public int ClientTierId { get; set; }
        
        public int MerchantTierId { get; set; }
        
        public int ClientId { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public decimal ClientCommission { get; set; }
        
        public decimal MemberCommission { get; set; }
        
        public int Status { get; set; }
    }
}