using System;

namespace Cashrewards3API.Features.Transaction.Model
{
    public class SaleAdjustmentTransactionResultModel
    {
        public decimal TotalSaleValueAud { get; set; }
        public decimal SaleValueAud { get; set; }
        public decimal CashbackValueAud { get; set; }
        public int TransactionId { get; set; }
        public int NetworkId { get; set; }
        public int MerchantId { get; set; }
        public int TransactionStatusId { get; set; }
        public int Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateApproved { get; set; }
        public DateTime SaleDate { get; set; }
        public int MemberId { get; set; }
        public int TransactionTypeId { get; set; }
        public bool IsLocked { get; set; }
        public bool IsMasterLocked { get; set; }
        public int ClientId { get; set; }
        public string AccessCode { get; set; }
        public int MerchantTransactionReportingTypeId { get; set; }
        public int ClickId { get; set; }
        public bool IsFirstTransaction { get; set; }
        public int[] CategoryIds{ get; set; }
        public DateTime DateJoined { get; set; }
    }
}