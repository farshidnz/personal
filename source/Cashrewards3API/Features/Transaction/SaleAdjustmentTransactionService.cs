using Cashrewards3API.Common.Services;
using Cashrewards3API.Features.Transaction.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Transaction
{
    public interface ISaleAdjustmentTransactionService
    {
        Task<IEnumerable<SaleAdjustmentTransactionResultModel>> GetTransactionDetailAsync(int transactionId, int? clickId);
    }

    public class SaleAdjustmentTransactionService : ISaleAdjustmentTransactionService
    {
        private readonly IReadOnlyRepository _readOnlyRepository;
        private readonly ITransactionService _transactionService;

        public SaleAdjustmentTransactionService(
            IReadOnlyRepository repository,
            ITransactionService transactionService)
        {
            _readOnlyRepository = repository;
            _transactionService = transactionService;
        }

        public async Task<IEnumerable<SaleAdjustmentTransactionResultModel>> GetTransactionDetailAsync(int transactionId, int? clickId) =>
            clickId.HasValue
                ? await GetTransactionDetailByClickIdAsync(clickId.Value)
                : await GetTransactionDetailByTransactionIdAsync(transactionId);

        #region dbqueries

        private async Task<IEnumerable<SaleAdjustmentTransactionResultModel>> GetTransactionDetailByClickIdAsync(int clickId)
        {
            const string queryString = @"
                SELECT
                    (select sum((IT.TransExchangeRate * itv.SaleValue))
                    FROM [Transaction] IT
                            JOIN [TransactionView] itv ON IT.TransactionId = itv.TransactionId
                    where ClickId = t.ClickId)  AS totalSaleValueAud,
                    (t.TransExchangeRate * tv.SaleValue) AS SaleValueAud,
                    tv.MemberCommissionValueAUD AS CashbackValueAud,
                    t.TransactionId,
                    t.NetworkId,
                    t.MerchantId,
                    t.TransactionStatusId,
                    t.Status,
                    t.DateCreatedUtc AS DateCreated,
                    t.DateApprovedUtc AS DateApproved,
                    COALESCE(mc.DateCreatedUtc , t.SaleDateUtc) as SaleDate,
                    t.MemberId,
                    t.TransactionTypeId,
                    t.IsLocked,
                    t.IsMasterLocked,
                    t.ClientId,
                    m.AccessCode,
                    mer.MerchantTransactionReportingTypeId,
                    mc.ClickId,
                    m.DateJoinedUtc As DateJoined
                FROM MemberClicks mc
                    JOIN [Transaction] t ON  t.ClickId = mc.ClickId
                    JOIN [TransactionView] tv ON t.TransactionId = tv.TransactionId
                    JOIN Member m ON t.MemberId = m.MemberId
                    JOIN Merchant mer on t.MerchantId = mer.MerchantId
                WHERE mc.ClickId = @clickId   AND tv.MemberCommissionValueAUD > 0;";

            var results = (await _readOnlyRepository.QueryAsync<SaleAdjustmentTransactionResultModel>(queryString,
                new
                {
                    clickId
                })).ToList();

            return await GetSaleAdjustmentAdditionalInfo(results);
        }

        private async Task<IEnumerable<SaleAdjustmentTransactionResultModel>> GetTransactionDetailByTransactionIdAsync(int transactionId)
        {
            const string queryString = @"
                SELECT
                    (t.TransExchangeRate * tv.SaleValue) AS totalSaleValueAud,
                    (t.TransExchangeRate * tv.SaleValue) AS saleValueAud,
                    tv.MemberCommissionValueAUD,
                    t.TransactionId,
                    t.NetworkId,
                    t.MerchantId,
                    t.TransactionStatusId,
                    t.Status,
                    t.DateCreatedUtc AS DateCreated,
                    t.DateApprovedUtc AS DateApproved,
                    COALESCE(mc.DateCreatedUtc , t.SaleDateUtc) as SaleDate,
                    t.MemberId,
                    t.TransactionTypeId,
                    t.IsLocked,
                    t.IsMasterLocked,
                    t.ClientId,
                    m.AccessCode,
                    mer.MerchantTransactionReportingTypeId,
                    t.ClickId,
                    m.DateJoinedUtc AS DateJoined
                FROM [Transaction] t 
                    JOIN [TransactionView] tv ON t.TransactionId = tv.TransactionId
                    JOIN Member m ON t.MemberId = m.MemberId
                    JOIN Merchant mer on t.MerchantId = mer.MerchantId
                    LEFT JOIN MemberClicks mc on t.ClickId = mc.ClickId
                WHERE t.TransactionId = @transactionId and tv.MemberCommissionValueAUD > 0";

            var results = (await _readOnlyRepository.QueryAsync<SaleAdjustmentTransactionResultModel>(queryString,
                new
                {
                    transactionId
                })).ToList();

            return await GetSaleAdjustmentAdditionalInfo(results);
        }

        private async Task<List<SaleAdjustmentTransactionResultModel>> GetSaleAdjustmentAdditionalInfo(List<SaleAdjustmentTransactionResultModel> models)
        {
            if (!models.Any())
            {
                return new List<SaleAdjustmentTransactionResultModel>();
            }

            // Since all transactions of a click id are with same merchant and member we can just 
            // pick the first one to get the member and merchant id.
            var memberId = models.First().MemberId;
            var merchantId = models.First().MerchantId;
            
            var firstPurchaseTransactionId = await _transactionService.GetMemberFirstPurchaseTransactionId(memberId);
            var merchantIds = (await GetCategoriesByMerchantIdAsync(merchantId)).ToArray();

            return models.Select(x =>
                {
                    x.IsFirstTransaction = firstPurchaseTransactionId == x.TransactionId;
                    x.CategoryIds = merchantIds;
                    return x;
                }
            ).ToList(); 
        }

        private async Task<IEnumerable<int>> GetCategoriesByMerchantIdAsync(int merchantId)
        {
            const string queryString = @"select CategoryId from MerchantCategoryMap where MerchantId = @merchantId";

            return await _readOnlyRepository.QueryAsync<int>(queryString,
                new
                {
                    merchantId
                });
        }

        #endregion
    }
}
