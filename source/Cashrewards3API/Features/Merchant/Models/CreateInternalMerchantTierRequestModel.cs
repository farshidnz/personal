using System;
using System.Collections.Generic;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class CreateInternalMerchantTierRequestModel
    {
        public CreateInternalMerchantTierRequestModel()
        {
            ClientIds = new List<int>();
        }

        public string TierName { get; set; }

        public string TierDescription { get; set; }

        public string TierReference { get; set; }

        public int MerchantId { get; set; }

        public decimal PromotionBonusValue { get; set; }

        public int PromotionBonusType { get; set; }

        public DateTime PromotionDateMin { get; set; }

        public DateTime PromotionDateMax { get; set; }

        public List<int> ClientIds { get; set; }
    }
}