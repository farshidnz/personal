using System;

namespace Cashrewards3API.Internals.BonusTransaction.Models
{
    public class BonusTransactionResultModel
    {
        public int TransactionId { get; set; }

        public string TransactionReference { get; set; }
        
        public int NetworkId { get; set; }
        
        public int MerchantId { get; set; }

        public int ClientId { get; set; }
        
        public int MemberId { get; set; }
        
        public int TransactionStatusId { get; set; }
        
        public int? GSTStatusId { get; set; }
        
        public int TransCurrencyId { get; set; }
        
        public DateTime SaleDate { get; set; } 
        
        public DateTime SaleDateAest { get; set; }
        
        public decimal TransExchangeRate { get; set; }
        
        public int Status { get; set; }
        
        public int? NetworkTranStatusId { get; set; }
        
        public string Comment { get; set; }
        
        public DateTime? DateCreated { get; set; }
        
        public DateTime? DateApproved { get; set; }
        
        public bool IsLocked { get; set; }
        
        public bool IsMasterLocked { get; set; }
        
        public int? TransactionTypeId { get; set; }
        
        public string TransactionDisplayId { get; set; }
    }
}