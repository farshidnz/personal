using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using Amazon.S3.Model.Internal.MarshallTransformations;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Transaction.Model;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashrewards3API.Helpers;

namespace Cashrewards3API.Features.Transaction
{
    public interface IMemberTransactionService
    {
        Task<PagedList<MemberTransactionResultModel>> GetTransactionsForMemberAsync(
            MemberTransactionRequestInfoModel model);
    }

    public class MemberTransactionService : IMemberTransactionService
    {
        private readonly ShopgoDBContext shopgoDBContext;
        private readonly CommonConfig commonConfig;

        public MemberTransactionService(ShopgoDBContext shopgoDBContext,
            CommonConfig commonConfig)
        {
            this.shopgoDBContext = shopgoDBContext;
            this.commonConfig = commonConfig;
        }

        public async Task<PagedList<MemberTransactionResultModel>> GetTransactionsForMemberAsync(
            MemberTransactionRequestInfoModel request)
        {
            return await GetTransactionsForMemberFromDbAsync(request);
        }

        private MemberTransactionResultModel ConvertToTransactionModel(
            MemberTransactionMerchantModel transactionMerchant)
        {
            var src = transactionMerchant;
            return new MemberTransactionResultModel
            {
                TransactionId = src.TransactionId,
                SaleDate = src.SaleDate,
                MerchantName = src.MerchantName,
                TransactionType = GetTransactionTypeString(src.TransactionType),
                Amount = FormattedSaleValue(src),
                Currency = src.CurrencyCode,
                Commission = FormattedCashback(src),
                Status = src.TransactionStatus,
                EstimatedApprovalDate = GetEstimatedApprovalDate(src),
                IsConsumption = Convert.ToBoolean(src.IsConsumption),
                MerchantLogoUrl = src.BackgroundImageUrl,
                ClientVerification = src.ClientVerificationTypeId != 0
                    ? src.ClientVerificationTypeId.GetEnumDescription()
                    : string.Empty
            };
        }

        private string GetEstimatedApprovalDate(MemberTransactionMerchantModel transaction)
        {
            string approvalDate = "";
            const string pendingStatus = "Pending";
            const string approvedStatus = "Approved";
            const string declinedStatus = "Declined";

            var datealert = transaction.SaleDate.AddDays(transaction.ApprovalWaitDays).ToShortDateString();

            if (transaction.TransactionStatus == pendingStatus && transaction.IsConsumption == 1)
            {
                approvalDate = transaction.ApprovalWaitDays + " days from travel completion";
            }
            else if (transaction.TransactionStatus == pendingStatus &&
                     transaction.MerchantId == commonConfig.Transaction.CashrewardsReferAMateMerchantId)
            {
                if (transaction.TransactionTypeId == (int) TransactionTypeEnum.PromoReferAMate)
                {
                    approvalDate = "Approved once your qualifying purchase is confirmed";
                }
                else if (transaction.TransactionTypeId == (int) TransactionTypeEnum.PromoReferAMateReferrer)
                {
                    approvalDate = "Approved once your friend's qualifying purchase is confirmed";
                }
            }
            else if (transaction.TransactionStatus == pendingStatus &&
                     (transaction.MerchantId == commonConfig.Transaction.CashrewardsBonusMerchantId ||
                      transaction.MerchantId == commonConfig.Transaction.CashrewardsActivationBonusMerchantId || 
                      transaction.MerchantId == commonConfig.Transaction.CashrewardsLegacyBonusMerchantId))
            {
                approvalDate = "Approved once your qualifying purchase is confirmed";
            }
            else if (commonConfig.Transaction.GiftCardMerchantIds.Split(',').Select(Int32.Parse)
                .Contains(transaction.MerchantId))
            {
                approvalDate = "-";
            }
            else if (transaction.TransactionStatus == pendingStatus &&
                     Convert.ToDateTime(datealert).Date > DateTime.Now.Date)
            {
                approvalDate = datealert;
            }
            else if (transaction.TransactionStatus == pendingStatus &&
                     Convert.ToDateTime(datealert).Date <= DateTime.Now.Date)
            {
                approvalDate = datealert;
            }
            else if (transaction.TransactionStatus == approvedStatus && !string.IsNullOrEmpty(transaction.DateApproved))
            {
                approvalDate = Convert.ToDateTime(transaction.DateApproved).ToShortDateString();
            }
            else if (transaction.TransactionStatus == approvedStatus && string.IsNullOrEmpty(transaction.DateApproved))
            {
                approvalDate = "-";
            }
            else if (transaction.TransactionStatus == declinedStatus && !string.IsNullOrEmpty(transaction.DateApproved))
            {
                approvalDate = Convert.ToDateTime(transaction.DateApproved).ToShortDateString();
            }
            else if (transaction.TransactionStatus == declinedStatus && string.IsNullOrEmpty(transaction.DateApproved))
            {
                approvalDate = "-";
            }
            else if (string.IsNullOrEmpty(transaction.DateApproved))
            {
                approvalDate = "-";
            }
            else
            {
                approvalDate = Convert.ToDateTime(transaction.DateApproved).ToShortDateString();
            }

            return approvalDate;
        }

        private string GetTransactionTypeString(int TransactionType)
        {
            var transactionTypeEnum = (TransactionTypeStringEnum) TransactionType;
            return transactionTypeEnum.ToString();
        }

        public decimal FormattedSaleValue(MemberTransactionMerchantModel transaction)
        {
            if (transaction?.SaleValue == null) return 0.00M;
            if (transaction.IsPaid == 1 || transaction.IsPaymentPending == 1 || transaction.IsDeclined == 1)
                transaction.SaleValue = -1 * transaction.SaleValue;
            return transaction.SaleValue.RoundToTwoDecimalPlaces();
        }

        private decimal FormattedCashback(MemberTransactionMerchantModel transaction)
        {
            if (transaction?.MemberCommissionValueAUD == null) return 0.00M;
            if (transaction.IsPaid == 1 || transaction.IsPaymentPending == 1 || transaction.IsDeclined == 1)
                transaction.MemberCommissionValueAUD = 0;
            return transaction.MemberCommissionValueAUD.RoundToTwoDecimalPlaces();
        }


        #region db queries

        private async Task<PagedList<MemberTransactionResultModel>> GetTransactionsForMemberFromDbAsync(
            MemberTransactionRequestInfoModel request)
        {
            string queryString = $@"SELECT trans.*, mer.BackgroundImageUrl, mer.BannerImageUrl,  
                                  ISNULL(TransactionClientVerification.ClientVerificationTypeId, 104) AS ClientVerificationTypeId 
                                  FROM TransactionView trans 
                                  INNER JOIN Merchant mer on trans.MerchantId = mer.MerchantId  
                                  LEFT OUTER JOIN TransactionClientVerification on TransactionClientVerification.TransactionId = trans.TransactionId
                                  WHERE trans.MemberId = @memberId
                                  ORDER BY trans.TransactionId
                                  OFFSET @offset ROWS
                                  FETCH NEXT @limit ROWS ONLY;

                                  SELECT count(1) 
                                  FROM TransactionView trans 
                                  INNER JOIN Merchant mer on trans.MerchantId = mer.MerchantId  
                                  LEFT OUTER JOIN TransactionClientVerification on TransactionClientVerification.TransactionId = trans.TransactionId
                                  WHERE trans.MemberId = @memberId;
                                ";

            using var conn = shopgoDBContext.CreateReadOnlyConnection();
            var trnasctionsResponse = await conn.QueryMultipleAsync(queryString,
                new
                {
                    memberId = request.MemberId,
                    offset = request.Offset,
                    limit = request.Limit
                });

            var transactionMerchantMoedls =
                (await trnasctionsResponse.ReadAsync<MemberTransactionMerchantModel>()).ToList();
            int totalCount = await trnasctionsResponse.ReadFirstAsync<int>();
            var transactions = transactionMerchantMoedls.Select(trxn => ConvertToTransactionModel(trxn)).ToList();
            return new PagedList<MemberTransactionResultModel>(totalCount, transactions.Count, transactions);
        }

        #endregion
    }
}