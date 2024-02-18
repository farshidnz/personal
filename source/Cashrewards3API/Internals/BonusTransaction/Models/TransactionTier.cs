namespace Cashrewards3API.Internals.BonusTransaction.Models
{
    public class TransactionTier
    {
        public int TransactionTierId { get; set; }
         
        public int TransactionId { get; set; }
        
        public string TierReferenceId { get; set; }
        
        public int MerchantTierId { get; set; }
        
        public decimal OperatingCommissionAud { get; set; }
        
        public string ConditionUsed { get; set; }
        
        public decimal MemberCommissionValueAud { get; set; }

        public static TransactionTier Create(CreateTransactionTierRequestModel requestModel)
            => new TransactionTier
            {
                TransactionId = requestModel.TransactionId,
                TierReferenceId = requestModel.TierReferenceId,
                MerchantTierId = requestModel.MerchantTierId,
                OperatingCommissionAud = requestModel.OperatingCommissionAud,
                ConditionUsed = requestModel.ConditionUsed,
                MemberCommissionValueAud = requestModel.MemberCommissionValueAud,
            };
    }
}