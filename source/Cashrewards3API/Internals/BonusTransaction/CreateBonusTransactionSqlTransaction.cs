using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Internals.BonusTransaction.Models;
using Dapper;

namespace Cashrewards3API.Internals.BonusTransaction
{
    /// <summary>
    /// A class to help execute the creation of a bonus transaction and its associated models
    /// (transaction tier, merchant tier and merchant tier client)
    /// in a single sql transaction.
    /// </summary>
    public class CreateBonusTransactionSqlTransaction
    {
        private readonly SqlConnection _conn;
        private readonly SqlTransaction _transaction;
        private readonly IMerchantService _merchantService;

        public CreateBonusTransactionSqlTransaction(
            SqlConnection conn,
            SqlTransaction transaction,
            IMerchantService merchantService)
        {
            _transaction = transaction;
            _conn = conn;
            _merchantService = merchantService;
        }


        /// <summary>
        /// Executes a sql transaction to create bonus transaction (A transaction), transaction tier,
        /// merchant tier client and potentially a merchant tier if existing one does not exist.
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="bonusTransactionToCreate"></param>
        /// <returns>Ids of the created/existing models</returns>
        public async Task<BonusTransactionCreateSqlTransactionResult> Execute(
            CreateBonusTransactionRequestModel requestModel,
            Models.BonusTransaction bonusTransactionToCreate)
        {
            var merchantTier = await _merchantService.GetMerchantTierById(
                requestModel.MerchantTierId,
                _conn,
                _transaction);

            if (merchantTier == null)
            {
                throw new Exception(
                    $"Merchant tier {requestModel.MerchantTierId} not found");
            }

            var bonusTransactionId = await SaveBonusTransactionToDb(bonusTransactionToCreate);
            var transactionTierId =
                await CreateTransactionTier(requestModel, bonusTransactionId, requestModel.MerchantTierId);

            return new BonusTransactionCreateSqlTransactionResult
            {
                TransactionId = bonusTransactionId,
                TransactionTierId = transactionTierId,
            };
        }

        private async Task<int> CreateTransactionTier(CreateBonusTransactionRequestModel requestModel,
            int bonusTransactionId, int merchantTierId)
            => await SaveTransactionTierToDb(TransactionTier.Create(new CreateTransactionTierRequestModel
            {
                TransactionId = bonusTransactionId,
                TierReferenceId = requestModel.TierReference,
                MerchantTierId = merchantTierId,
                OperatingCommissionAud = 0,
                ConditionUsed = "0",
                MemberCommissionValueAud = requestModel.Promotion.BonusValue
            }));

        private async Task<int> SaveTransactionTierToDb(TransactionTier transactionTierToCreate)
        {
            const string insertTransactionTierQuery =
                @"INSERT INTO TransactionTier 
                    (TransactionId, TierReferenceId, MerchantTierId, OperatingCommissionAud, 
                    ConditionUsed, MemberCommissionValueAud) 
                VALUES 
                    (@TransactionId, @TierReferenceId, @MerchantTierId, @OperatingCommissionAud,
                    @ConditionUsed, @MemberCommissionValueAud)
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var insertResult =
                await _conn.QueryAsync<int>(insertTransactionTierQuery, transactionTierToCreate, _transaction);
            return insertResult.Single();
        }

        private async Task<int> SaveBonusTransactionToDb(Models.BonusTransaction bonusTransactionToCreate)
        {
            const string insertBonusTransactionQuery =
                @"INSERT INTO [Transaction] 
                    (TransactionReference, NetworkId, MerchantId, ClientId, MemberId, TransactionStatusId,
                    GSTStatusId, TransCurrencyId, SaleDate, SaleDateAest, TransExchangeRate, Status, NetworkTranStatusId, Comment,
                    DateCreated, DateApproved, IsLocked, IsMasterLocked, TransactionTypeId, TransactionDisplayId) 
                VALUES
                    (@TransactionReference, @NetworkId, @MerchantId, @ClientId, @MemberId, @TransactionStatusId,
                    @GSTStatusId, @TransCurrencyId, @SaleDate, @SaleDateAest, @TransExchangeRate, @Status, @NetworkTranStatusId, @Comment,
                    @DateCreated, @DateApproved, @IsLocked, @IsMasterLocked, @TransactionTypeId, @TransactionDisplayId);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var insertResult =
                await _conn.QueryAsync<int>(insertBonusTransactionQuery, bonusTransactionToCreate, _transaction);
            return insertResult.Single();
        }
    }
}