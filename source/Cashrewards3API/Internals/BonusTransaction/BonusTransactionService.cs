using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Features.Member.Repository;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.ShopGoClient;
using Cashrewards3API.Internals.BonusTransaction.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Internals.BonusTransaction
{
    public interface IBonusTransactionService
    {
        Task<BonusTransactionResultModel> CreateBonusTransaction(CreateBonusTransactionRequestModel requestModel);
        Task<BonusTransactionResultModel> ApproveBonusTransaction(int transactionId);
        Task<BonusTransactionResultModel> GetById(int transactionId);
        Task<BonusTransactionResultModel> DeclineBonusTransaction(int transactionId);
        Task<IEnumerable<QualifyingTransactionsResultModel>> GetQualifyingTransactions(QualifyingTransactionsRequestModel request);
    }

    public class BonusTransactionService : IBonusTransactionService
    {
        private readonly ShopgoDBContext _shopGoDbContext;
        private readonly IMapper _mapper;
        private readonly CommonConfig _commonConfig;
        private readonly IMerchantService _merchantService;
        private readonly IShopGoClientService _shopGoClientService;
        private readonly ILogger<BonusTransactionService> _logger;
        private readonly IMemberRepository _memberRepository;

        public BonusTransactionService(ShopgoDBContext shopGoDbContext, IMapper mapper, CommonConfig commonConfig,
            IMerchantService merchantService, IShopGoClientService shopGoClientService, ILogger<BonusTransactionService> logger, IMemberRepository memberRepository)
        {
            _shopGoDbContext = shopGoDbContext;
            _mapper = mapper;
            _commonConfig = commonConfig;
            _merchantService = merchantService;
            _shopGoClientService = shopGoClientService;
            _logger = logger;
            _memberRepository = memberRepository;
        }

        public async Task<BonusTransactionResultModel> CreateBonusTransaction(
            CreateBonusTransactionRequestModel requestModel)
        {
            var timeZoneInfo = await _merchantService.GetTimezoneInfoForMerchant(requestModel.MerchantId);
            var bonusTransaction = Models.BonusTransaction.Create(requestModel, _commonConfig.Transaction, timeZoneInfo);
            
            var clients = await _shopGoClientService.GetShopGoClients();
            if (clients.All(c => c.ClientId != bonusTransaction.ClientId))
            {
                throw new Exception($"Client id {bonusTransaction.ClientId} does not exist");
            }

            await using var conn = _shopGoDbContext.CreateConnection();
            conn.Open();
            await using var transaction = conn.BeginTransaction();

            var createBonusTransactionSqlTransaction =
                new CreateBonusTransactionSqlTransaction(conn, transaction, _merchantService);
            var result =
                await createBonusTransactionSqlTransaction.Execute(requestModel, bonusTransaction);

            transaction.Commit();

            LogBonusTransactionIds(result);
            return await GetById(result.TransactionId);
        }

        public async Task<BonusTransactionResultModel> ApproveBonusTransaction(int transactionId)
        {
            var bonusTransactionToApprove = Models.ApproveBonusTransaction.Create(transactionId);
            const string approveBonusTransactionQuery =
                @"UPDATE [Transaction] 
                    SET TransactionStatusId = @TransactionStatusId, NetworkTranStatusId = @NetworkTranStatusId, 
                        LastUpdated=@LastUpdated, DateApproved = @DateApproved
                WHERE 
                    (TransactionId = @TransactionId AND TransactionStatusId != @TransactionStatusDeclined  AND IsLocked = 0 );";

            await using var conn = _shopGoDbContext.CreateConnection();
            var totalRowsAffected =
                await conn.ExecuteAsync(approveBonusTransactionQuery, bonusTransactionToApprove);

            return totalRowsAffected > 0
                ? await GetById(transactionId)
                : null;

        }

        public async Task<BonusTransactionResultModel> DeclineBonusTransaction(int transactionId)
        {
            var bonusTransactionToDecline = Models.DeclineBonusTransaction.Create(transactionId);
            const string declineBonusTransactionQuery =
                @"UPDATE [Transaction] 
                    SET TransactionStatusId = @TransactionStatusId, NetworkTranStatusId = @NetworkTranStatusId, 
                        LastUpdated=@LastUpdated
                WHERE 
                    (TransactionId = @TransactionId AND IsLocked = 0);";

            await using var conn = _shopGoDbContext.CreateConnection();
            var totalRowsAffected =
                await conn.ExecuteAsync(declineBonusTransactionQuery, bonusTransactionToDecline);

            return totalRowsAffected > 0
                ? await GetById(transactionId)
                : null;
        }

        public async Task<BonusTransactionResultModel> GetById(int transactionId)
        {
            const string getByIdQuery =
                @"SELECT * FROM [Transaction] WHERE TransactionId=@TransactionId";

            await using var conn = _shopGoDbContext.CreateConnection();
            var bonusTransaction = await conn.QueryFirstOrDefaultAsync<Models.BonusTransaction>(getByIdQuery, new
            {
                TransactionId = transactionId
            });

            return _mapper.Map<BonusTransactionResultModel>(bonusTransaction);
        }

        private void LogBonusTransactionIds(BonusTransactionCreateSqlTransactionResult result)
            => _logger.LogInformation(
                $@"BonusTransactionId: {result.TransactionId} TransactionTierId: {result.TransactionTierId}");


        public async Task<IEnumerable<QualifyingTransactionsResultModel>> GetQualifyingTransactions(QualifyingTransactionsRequestModel request)
        {
            var qualifyingTransactionsRules = QualifyingTransactionsRules.Create(request);
            var associatedMembers = await _memberRepository.GetAssociatedMemberIds(request.MemberId);
            if (!associatedMembers.Any())
                 associatedMembers = new List<int> {request.MemberId};

            qualifyingTransactionsRules.AssoicatedMemberIds = associatedMembers;

            string getQualifyingTransactionsQuery = $@"SELECT 
                COALESCE(
                    CASE WHEN [MerchantTransactionReportingTypeId] = {Constants.MerchantReportingType.SaleValueNotSupplied} AND 
                        [TransactionStatusId]  = 101 THEN 1 END,
                    CASE WHEN [MerchantTransactionReportingTypeId] = {Constants.MerchantReportingType.SaleValueNotSupplied} AND 
                        [TransactionStatusId]  = 100 THEN 0 END , 
                    CASE WHEN [totalApprovedSaleValueAud] >= {getGstAdjustedValue(qualifyingTransactionsRules.SaleValueMin, request.HasGst)} THEN 1 
                        ELSE 0 END 
                ) [canApprove],
                       *
                FROM (SELECT CASE WHEN t.ClickId IS NOT NULL THEN 
                            (SELECT SUM(([IT].[TransExchangeRate] * [itv].[SaleValue]))
                              FROM [Transaction][IT]
                                       JOIN[TransactionView][itv] ON[IT].[TransactionId] = [itv].[TransactionId]
                                       JOIN [MemberClicks] mc on [IT].[ClickId] = [mc].[ClickId]
                              WHERE [IT].[ClickId] = [t].[ClickId] 
                                AND [mc].[DateCreatedUtc] BETWEEN @StartDate AND @EndDate)
                            ELSE 
                                (t.TransExchangeRate * tv.  SaleValue)    END  AS[totalSaleValueAud],
                            CASE WHEN t.ClickId IS NOT NULL THEN 
                                (SELECT SUM(([IT].[TransExchangeRate] * [itv].[SaleValue]))
                                    FROM[Transaction][IT]
                                       JOIN[TransactionView][itv] ON[IT].[TransactionId] = [itv].[TransactionId]
                              WHERE [IT].[ClickId] = [t].[ClickId]
                                AND[IT].[TransactionStatusId] = 101
                                AND [mc].[DateCreatedUtc] BETWEEN @StartDate AND @EndDate) 
                            ELSE  
                                CASE WHEN [t].[TransactionStatusId]  = 101 THEN (t.TransExchangeRate * tv.  SaleValue) ELSE 0 END    
                            END   
                                AS[totalApprovedSaleValueAud],
                             ([t].[TransExchangeRate] * [tv].[SaleValue]) AS[saleValueAud],
                             CASE WHEN DATEDIFF(DAY, [m].[DateJoinedUtc], COALESCE([mc].[DateCreatedUtc], [t].[SaleDateAest])) <= 0 THEN 0 ELSE 
                             DATEDIFF(DAY, [m].[DateJoinedUtc], COALESCE([mc].[DateCreatedUtc], [t].[SaleDateAest])) END  AS[firstPurchaseWindow],
                             [m].[DateJoined],
                             [t].[MerchantId],
                             [t].[TransactionStatusId],
                             [t].[TransactionId],
                             [t].NetworkId,
                             [mer].[MerchantTransactionReportingTypeId],
                             CASE WHEN [m].[DateJoined] < (CASE WHEN [mc].DateCreatedUtc is not null THEN [mc].DateCreatedUtc ELSE [t].SaleDateUtc END)  
                                                     THEN (CASE WHEN [mc].DateCreatedUtc is not null THEN [mc].DateCreatedUtc ELSE [t].SaleDateUtc END) 
                                                     ELSE  [m].[DateJoined] END as SaleDate,
                             [t].ClientId
                      FROM [Transaction][t]
                               JOIN [TransactionView][tv] ON[t].[TransactionId] = [tv].[TransactionId]
                               JOIN [Member][m] ON[t].[MemberId] = [m].[MemberId]
                               JOIN [Merchant][mer] ON[t].[MerchantId] = [mer].[MerchantId]
                               LEFT JOIN[MemberClicks] [mc] ON[t].[ClickId] = [mc].[ClickId]
                      WHERE [tv].[MemberCommissionValueAUD] > 0
                        AND [m].[MemberId] IN @AssoicatedMemberIds
                       AND [t].[TransactionStatusId] IN(100, 101)) [a]";

            var sale_Value = request?.New_Transaction?.Sale_Value;
            List<string> where = new List<string>();
            where.Add(@"[SaleDate] BETWEEN @StartDate AND @EndDate");
            if (sale_Value != null)
            {
                if (sale_Value.Min.HasValue)
                {
                    var SaleValueMin = getGstAdjustedValue(qualifyingTransactionsRules.SaleValueMin, request.HasGst);
                    where.Add($@"([totalSaleValueAud] >= CASE WHEN [MerchantTransactionReportingTypeId] = {Constants.MerchantReportingType.SaleValueNotSupplied} THEN 0 ELSE  {SaleValueMin} END OR 
                                  [saleValueAud] >= CASE WHEN [MerchantTransactionReportingTypeId] = {Constants.MerchantReportingType.SaleValueNotSupplied} THEN 0 ELSE  {SaleValueMin} END )");
                }
                else if (sale_Value.Max.HasValue)
                {
                    var SaleValueMax = getGstAdjustedValue(qualifyingTransactionsRules.SaleValueMax, request.HasGst);
                    where.Add($"([totalSaleValueAud] <= {SaleValueMax} OR [saleValueAud] <= {SaleValueMax} )");
                }
            }

            var merchant_Id = request?.New_Transaction?.Merchant_Id;
            if (merchant_Id != null)
            {
                if (!string.IsNullOrEmpty(merchant_Id.In))
                {
                    where.Add($" ([MerchantId] IN ({merchant_Id.In} ))");
                }
                else if (!string.IsNullOrEmpty(merchant_Id.Not_In))
                {
                    where.Add($" ([MerchantId] NOT IN ({merchant_Id.Not_In} ))");
                }
            }

            var member_Joined_Date = request?.New_Transaction?.Member_Joined_Date;
            if (member_Joined_Date != null)
            {
                if (member_Joined_Date.Before != null)
                {
                    where.Add($" ([DateJoined] <  '{TimeZoneInfo.ConvertTime(member_Joined_Date.Before.Value, TimeZoneInfo.Utc, Constants.SydneyTimezone):yyyy-MM-dd HH:mm:ss}')");
                }

                if (member_Joined_Date.After != null)
                {
                    where.Add($" ([DateJoined] >  '{TimeZoneInfo.ConvertTime(member_Joined_Date.After.Value, TimeZoneInfo.Utc, Constants.SydneyTimezone):yyyy-MM-dd HH:mm:ss}')");
                }
            }

            var first_Purchase_Window = request?.New_Transaction?.First_Purchase_Window;
            if (first_Purchase_Window != null)
            {
                if (first_Purchase_Window.Max != null)
                {
                    where.Add(@$" ([firstPurchaseWindow] < {first_Purchase_Window.Max} )");
                }
            }

            var Category_Id = request?.New_Transaction?.Category_Id;
            if (Category_Id != null)
            {
                if (Category_Id.In != null)
                {
                    where.Add($@" ([MerchantId] IN   (
                        SELECT  DISTINCT mcm.MerchantId FROM MerchantCategoryMap mcm where [mcm].MerchantId  = [a].MerchantId and CategoryId  in ({Category_Id.In})
                     ))");
                }
                else if (Category_Id.Not_In != null)
                {
                    where.Add($@" ([MerchantId] NOT IN   (
                        SELECT  DISTINCT mcm.MerchantId FROM MerchantCategoryMap mcm where [mcm].MerchantId  = [a].MerchantId and CategoryId  in ({Category_Id.Not_In})
                     ))");
                }
            }

            var Store_Type = request?.New_Transaction?.Store_Type;
            if (Store_Type != null)
            {
                if (Store_Type.In != null)
                {
                    var filter = getNetworkFilterForInClause(Store_Type.In);
                    if (!string.IsNullOrEmpty(filter))
                    where.Add(filter);
                }
                else if (Store_Type.Not_In != null)
                {
                    var filter = getNetworkFilterForNotInClause(Store_Type.Not_In);
                    if (!string.IsNullOrEmpty(filter))
                    where.Add(filter);
                }
            }

            var network_Id = request?.New_Transaction?.Network_Id;
            if (network_Id != null)
            {
                if (!string.IsNullOrEmpty(network_Id.In))
                {
                    where.Add($" [NetworkId] IN (@NetworkId_In) ");
                }
                else if (!string.IsNullOrEmpty(network_Id.Not_In))
                {
                    where.Add($" [NetworkId] NOT IN (@NetworkId_Not_In) ");
                }
            }

            if (where.Count > 0)
                getQualifyingTransactionsQuery += " WHERE " + string.Join(" AND ", where);

            await using var conn = _shopGoDbContext.CreateReadOnlyConnection();
            var qualifyingTransactions =
                await conn.QueryAsync<QualifyingTransactionsResultModel>(getQualifyingTransactionsQuery, qualifyingTransactionsRules);

            return qualifyingTransactions;
        }

        private decimal getGstAdjustedValue(decimal saleValue, bool hasGst = true)
        {
            if (!hasGst)
            {
                return saleValue;
            }
            var gst = saleValue * _commonConfig.GstConfig.Percentage;
            return (saleValue - gst) - _commonConfig.GstConfig.Adjustment;
        }

        private string getNetworkFilterForInClause(string storeType)
        {
            string filter = "";
            if (!storeType.Contains(","))
                if (storeType == "Instore")
                {
                    filter = $" NetworkId = {_commonConfig.InStoreNetworkId} ";
                }
                else if (storeType == "Online")
                {
                    filter = $" NetworkId != {_commonConfig.InStoreNetworkId} ";
                }
            return filter;
        }

        private string getNetworkFilterForNotInClause(string storeType)
        {
            string filter = "";
            if (!storeType.Contains(","))
                if (storeType == "Instore")
                {
                    filter = $" NetworkId != {_commonConfig.InStoreNetworkId} ";
                }
                else if (storeType == "Online")
                {
                    filter = $" NetworkId = {_commonConfig.InStoreNetworkId} ";
                }

            return filter;
        }
    }
}