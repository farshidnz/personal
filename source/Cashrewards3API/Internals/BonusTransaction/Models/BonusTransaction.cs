using System;
using System.Threading.Tasks;
using Cashrewards3API.Common;
using Cashrewards3API.Enum;

namespace Cashrewards3API.Internals.BonusTransaction.Models
{
    public class BonusTransaction
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

        public TransactionTypeEnum? TransactionTypeId { get; set; }

        public string TransactionDisplayId { get; set; }

        public bool NetworkTracked { get; set; }

        public static BonusTransaction Create(CreateBonusTransactionRequestModel requestModel, Transaction config,
            TimeZoneInfo timeZoneInfoId)
            => new BonusTransaction
            {
                TransactionReference = requestModel.TransactionReference,
                MemberId = requestModel.MemberId,
                SaleDate = TimeZoneInfo.ConvertTime(requestModel.DateActivated ?? DateTime.UtcNow, TimeZoneInfo.Utc, timeZoneInfoId),
                SaleDateAest = TimeZoneInfo.ConvertTime(requestModel.DateActivated ?? DateTime.UtcNow, TimeZoneInfo.Utc, Constants.SydneyTimezone),
                NetworkId = config.ShopGoNetworkId,
                MerchantId = requestModel.MerchantId,
                ClientId = requestModel.ClientId ?? config.CashrewardsClientId,
                TransactionStatusId = config.TransactionStatusPendingId,
                GSTStatusId = config.GstStatusExclusiveGstId,
                TransCurrencyId = config.CurrencyAudId,
                TransExchangeRate = 1m, // Since its Australian currency its 1:1
                Status = config.StatusPendingId,
                NetworkTranStatusId = config.NetworkTranStatusPendingId,
                DateCreated = TimeZoneInfo.ConvertTime(requestModel.DateActivated ?? DateTime.UtcNow, TimeZoneInfo.Utc, Constants.SydneyTimezone),
                IsLocked = false,
                IsMasterLocked = false,
                TransactionTypeId = TransactionTypeEnum.PromoMs,
                TransactionDisplayId = requestModel.TransactionDisplayId,
                NetworkTracked = false
            };
    }
}