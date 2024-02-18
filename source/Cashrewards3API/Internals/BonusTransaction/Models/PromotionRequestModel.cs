using System;

namespace Cashrewards3API.Internals.BonusTransaction.Models
{
    public class PromotionRequestModel
    {
        public DateTime PromotionDateMin { get; set; }
        
        public DateTime PromotionDateMax { get; set; }
        
        public decimal BonusValue { get; set; }
        
        public int BonusType { get; set; }
    }
}