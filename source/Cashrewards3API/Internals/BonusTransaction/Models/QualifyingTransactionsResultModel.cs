using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Internals.BonusTransaction.Models
{
    public class QualifyingTransactionsResultModel
    {
        public bool CanApprove { get; set; }
        public decimal TotalSaleValueAud { get; set; }
        public decimal TotalApprovedSaleValueAud { get; set; }
        public decimal SaleValueAud { get; set; }
        public int FirstPurchaseWindow { get; set; }
        public DateTime DateJoined { get; set; }
        public int MerchantId { get; set; }
        public int TransactionStatusId { get; set; }
        public int TransactionId { get; set; }
        public DateTime SaleDate { get; set; }
        public int ClientId { get; set; }
    }
}
