using System;

namespace Cashrewards3API.Internals.BonusTransaction.Models
{
    public class CreateBonusTransactionRequestModel
    {
        public string TransactionReference { get; set; }
        
        public string TierDescription { get; set; }
        
        public string TierReference { get; set; }
        
        public int MerchantId { get; set; }

        public int MemberId { get; set; }

        public string TransactionDisplayId { get; set; }
        
        public string TierName { get; set; }
        
        public PromotionRequestModel Promotion { get; set; }
        
        public int MerchantTierId { get; set; }

        public int? ClientId { get; set; }

        public DateTime? DateActivated { get; set; }
    }
}