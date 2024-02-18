namespace Cashrewards3API.Internals.BonusTransaction.Models
{
    public class TransactionTierResultModel
    {
        public int TransactionTierId { get; set; }
         
        public int TransactionId { get; set; }
        
        public string TierReferenceId { get; set; }
        
        public int MerchantTierId { get; set; }
        
        public decimal OperatingCommissionAud { get; set; }
        
        public string ConditionUsed { get; set; }
        
        public decimal MemberCommissionValueAud { get; set; }
    }
}