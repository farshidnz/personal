using System;
using Cashrewards3API.Enum;

namespace Cashrewards3API.Features.Transaction.Model
{
    public class MemberTransactionMerchantModel
    {
        public int TransactionId { get; set; }
        public DateTime SaleDate { get; set; }
        public int MerchantId { get; set; }
        public string MerchantName { get; set; }
        public int TransactionType { get; set; }
        public int TransactionTypeId { get; set; }
        public decimal SaleValue { get; set; }
        public string CurrencyCode { get; set; }
        public decimal MemberCommissionValueAUD { get; set; }
        public string TransactionStatus { get; set; }
        public int ApprovalWaitDays { get; set; }
        public int? IsConsumption { get; set; }
        public string DateApproved { get; set; }
        public int IsPaid { get; set; }
        public int IsPaymentPending { get; set; }
        public int IsDeclined { get; set; }
        public string BackgroundImageUrl { get; set; }
        public ClientVerificationType ClientVerificationTypeId { get; set; }
    }
}
