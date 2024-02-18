using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Transaction.Model
{
    public class MemberTransactionResultModel
    {
        public int TransactionId { get; set; }
        public DateTime SaleDate { get; set; }
        public string MerchantName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public decimal Commission { get; set; }
        public string Status { get; set; }
        public string TransactionType { get; set; }
        public string EstimatedApprovalDate { get; set; }
        public bool IsConsumption { get; set; }
        public string MerchantLogoUrl { get; set; }
        public string ClientVerification { get; set; }
    }
}
