using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Enum;
using Cashrewards3API.Exceptions;
using Cashrewards3API.Features.Merchant.Models;
using Dapper;
using System.Collections.Generic;
using Cashrewards3API.Common.Services;

namespace Cashrewards3API.Features.Merchant
{
    public class MerchantInternalService : IMerchantInternalService
    {
        private readonly CommonConfig _commonConfig;
        private readonly IMerchantService _merchantService;
        private readonly ShopgoDBContext _shopGoDbContext;
        private readonly IReadOnlyRepository _readOnlyRepository;
        private readonly IMapper _mapper;

        public MerchantInternalService(CommonConfig commonConfig,
            IMerchantService merchantService,
            ShopgoDBContext shopGoDbContext,
            IMapper mapper,
            IReadOnlyRepository readOnlyRepository)
        {
            _commonConfig = commonConfig;
            _merchantService = merchantService;
            _shopGoDbContext = shopGoDbContext;
            _mapper = mapper;
            _readOnlyRepository = readOnlyRepository;
        }

        public async Task<MerchantTier> CreateInternalMerchantTier(
            CreateInternalMerchantTierRequestModel requestModel)
        {
            MerchantTier merchantTier = _mapper.Map<MerchantTier>(requestModel, opts =>
            {
                opts.Items[Constants.Mapper.CurrencyId] = _commonConfig.Transaction.CurrencyAudId;
                opts.Items[Constants.Mapper.TierTypePromotionId] = _commonConfig.Promotion.TierTypePromotionId;
            });

            await using var conn = _shopGoDbContext.CreateConnection();
            conn.Open();
            await using var transaction = conn.BeginTransaction();

            var result = await CreateMerchantTier(merchantTier, requestModel, conn, transaction);

            await transaction.CommitAsync();

            return result;
        }

        public async Task UpdateInternalMerchantTier(UpdateInternalMerchantTierRequestModel requestModel)
        {
            MerchantTier updateMerchantRequest = _mapper.Map<MerchantTier>(requestModel);

            await using var conn = _shopGoDbContext.CreateConnection();
            conn.Open();
            await using var transaction = conn.BeginTransaction();

            await UpdateMerchantTier(updateMerchantRequest, requestModel.ClientIds.ToList(), conn, transaction);

            await transaction.CommitAsync();
        }

        public async Task DeactivateMerchantTier(int merchantTierId)
        {
            await using var conn = _shopGoDbContext.CreateConnection();
            await conn.OpenAsync();
            await using var transaction = conn.BeginTransaction();

            const string deactivateMerchantTierQuery =
                @"UPDATE MerchantTier
                    SET Status=@Status
                    WHERE MerchantTierId=@MerchantTierId;";
            await conn.QueryAsync<int>(deactivateMerchantTierQuery, new
            {
                Status = MerchantTierStatusTypeEnum.Inactive,
                MerchantTierId = merchantTierId
            }, transaction);

            const string deactivateMerchantTierClientQuery =
                @"UPDATE MerchantTierClient
                    SET Status=@Status
                    WHERE MerchantTierId=@MerchantTierId AND Status = 1;";
            await conn.QueryAsync<int>(deactivateMerchantTierClientQuery, new
            {
                Status = MerchantTierStatusTypeEnum.Inactive,
                MerchantTierId = merchantTierId
            }, transaction);

            await transaction.CommitAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="merchantTierId"></param>
        /// <param name="clientId"></param>
        /// <param name="dateTimeUtc"></param>
        /// <returns></returns>
        public async Task<Boolean> ExistsMerchantTierClient(int merchantTierId, int clientId, DateTime dateTimeUtc)
        {
            string existsMerchantTierClientQuery = @"
                SELECT 1 FROM MerchantTierClient
                    WHERE MerchantTierId = @MerchantTierId
                        AND ClientId = @ClientId
                        AND Status = 1
                        AND @DateTimeUtc BETWEEN StartDateUtc AND EndDateUtc;
            ";

            return (await _readOnlyRepository.Query<Boolean>(existsMerchantTierClientQuery, new
            {
                MerchantTierId = merchantTierId,
                ClientId = clientId,
                DateTimeUtc = dateTimeUtc,
            })).Contains(true);
        }

        private async Task<MerchantTier> CreateMerchantTier(
            MerchantTier merchantTier,
            CreateInternalMerchantTierRequestModel requestModel,
            SqlConnection conn,
            SqlTransaction transaction)
        {
            var existingMerchantTiers = await _merchantService.GetMerchantTiers(merchantTier.MerchantId,
                merchantTier.TierName, requestModel.PromotionDateMin,
                requestModel.PromotionDateMax, conn, transaction);

            if (existingMerchantTiers.Any())
                throw new BadRequestException("This Merchant Tier already Exist");

            if (!requestModel.ClientIds.Any())
                requestModel.ClientIds.Add(_commonConfig.Transaction.CashrewardsClientId);

            var createdMerchantTier = await InsertMerchantTierToDb(merchantTier, conn, transaction);
            MerchantTierClient merchantTierClient = null, createdMerchantTierClient = null;
            foreach (int clientId in requestModel.ClientIds)
            {
                merchantTierClient = _mapper.Map<MerchantTierClient>(createdMerchantTier, opts =>
                {
                    opts.Items[Constants.Mapper.ClientId] = clientId;
                });
                createdMerchantTierClient = await InsertMerchantTierClientToDb(merchantTierClient, conn, transaction);
            }
            createdMerchantTier.ClientIds = requestModel.ClientIds;
            return createdMerchantTier;
        }

        private async Task UpdateMerchantTier(MerchantTier merchantTier
            , List<int> clientIds
            , SqlConnection conn
            , SqlTransaction transaction)
        {
            var existingMerchantTier =
                await _merchantService.GetMerchantTierById(merchantTier.MerchantTierId, conn, transaction);

            if (existingMerchantTier == null)
                throw new NotFoundException($"MerchantTier {merchantTier.MerchantTierId} does not exist");

            MerchantTierClient merchantTierClient = null;

            List<int> clientsToDelete = existingMerchantTier.ClientIds.Except(clientIds).ToList();

            foreach (int clientsId in clientsToDelete)
            {
                merchantTierClient = _mapper.Map<MerchantTierClient>(merchantTier, opts =>
                {
                    opts.Items[Constants.Mapper.ClientId] = clientsId;
                });
                merchantTierClient.Status = (int)MerchantTierClientStatusTypeEnum.Deleted;
                await UpdateMerchantTierClientToDb(merchantTierClient, conn, transaction);
            }

            foreach (int clientId in clientIds)
            {
                merchantTierClient = _mapper.Map<MerchantTierClient>(merchantTier, opts =>
                {
                    opts.Items[Constants.Mapper.ClientId] = clientId;
                });
                if (existingMerchantTier.ClientIds.Contains(clientId))
                    await UpdateMerchantTierClientToDb(merchantTierClient, conn, transaction);
                else
                    if (!existingMerchantTier.ClientIds.Contains(clientId))
                    await InsertMerchantTierClientToDb(merchantTierClient, conn, transaction);
            }

            await UpdateMerchantTierToDb(merchantTier, conn, transaction);
        }

        private async Task<MerchantTier> InsertMerchantTierToDb(MerchantTier merchantTier,
            SqlConnection conn,
            SqlTransaction transaction)
        {
            const string insertMerchantTierQuery =
                @"INSERT INTO MerchantTier
                    (MerchantId, TierName, TierDescription, Commission, TierTypeId, TierCommTypeId,
                    StartDate, EndDate, Status, TrackingLink, TierReference, IsAdvancedTier, CurrencyId)
                VALUES
                    (@MerchantId, @TierName, @TierDescription, @Commission, @TierTypeId, @TierCommTypeId,
                    @StartDate, @EndDate, @Status, @TrackingLink, @TierReference, @IsAdvancedTier, @CurrencyId);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var insertedResult = await conn.QueryAsync<int>(insertMerchantTierQuery, merchantTier, transaction);
            return await _merchantService.GetMerchantTierById(insertedResult.Single(), conn, transaction);
        }

        private async Task<MerchantTierClient> InsertMerchantTierClientToDb(MerchantTierClient merchantTierClient,
            SqlConnection conn,
            SqlTransaction transaction)
        {
            const string insertMerchantTierClientQuery =
                @"INSERT INTO MerchantTierClient
                    (MerchantTierId, ClientId, StartDate, EndDate, ClientComm, MemberComm, Status)
                VALUES
                    (@MerchantTierId, @ClientId, @StartDate, @EndDate, @ClientCommission, @MemberCommission, @Status);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var insertedResult =
                await conn.QueryAsync<int>(insertMerchantTierClientQuery, merchantTierClient, transaction);
            return await GetMerchantTierClientById(insertedResult.Single(), conn, transaction);
        }

        private async Task UpdateMerchantTierToDb(MerchantTier merchantTier, SqlConnection conn,
            SqlTransaction transaction)
        {
            const string updateMerchantTierQuery =
                @"UPDATE MerchantTier
        SET TierName=@TierName, TierDescription=@TierDescription, Commission=@Commission,
        TierCommTypeId=@TierCommTypeId, StartDate=@StartDate, EndDate=@EndDate, TierReference=@TierReference
        WHERE MerchantTierId=@MerchantTierId;";
            await conn.QueryAsync<int>(updateMerchantTierQuery, merchantTier, transaction);
        }

        private async Task UpdateMerchantTierClientToDb(MerchantTierClient merchantTierClient, SqlConnection conn,
            SqlTransaction transaction)
        {
            const string updateMerchantTierClientQuery =
                @"UPDATE MerchantTierClient
        SET MerchantTierId=@MerchantTierId, StartDate=@StartDate, EndDate=@EndDate , Status =@Status
        WHERE MerchantTierId=@MerchantTierId AND ClientId=@ClientId AND Status = 1;";

            await conn.QueryAsync<int>(updateMerchantTierClientQuery, merchantTierClient, transaction);
        }

        private async Task<MerchantTierClient> GetMerchantTierClientById(int id, SqlConnection conn,
            SqlTransaction transaction)
        {
            const string getByIdQuery = @"SELECT * FROM MerchantTierClient WHERE ClientTierId=@Id";
            var result = await conn.QueryFirstOrDefaultAsync<MerchantTierClient>(getByIdQuery, new
            {
                Id = id,
            }, transaction);

            return result;
        }

        /// <summary>
        /// Returns active merchant tiers for a merchant at a specified time
        /// </summary>
        /// <param name="merchantId">Cashrewards merchantid</param>
        /// <param name="dateTimeUtc">time to check for a merchant tier by merchantId</param>
        /// <param name="top">Only return this many matches</param>
        /// <returns>List of active merchant tiers</returns>
        public async Task<IList<MerchantTierEftposTransformer>> GetActiveMerchantTiers(int merchantId, DateTime dateTimeUtc, int? top = 0)
        {
            return await GetMerchantTiers(merchantId, dateTimeUtc, MerchantTierStatusTypeEnum.Active, top);
        }

        /// <summary>
        /// Returns merchant tiers for a merchant at a specified time with the specified status
        /// An adaptor to turn a single status into an array to use the generic function
        /// </summary>
        /// <param name="merchantId">Cashrewards merchantid</param>
        /// <param name="dateTimeUtc">time to check for a merchant tier by merchantId</param>
        /// <param name="status">Status to check. Active/Inactive/Deleted</param>
        /// <param name="top">Only return this many matches</param>
        /// <returns>List of merchant tiers matching the status</returns>
        private async Task<IList<MerchantTierEftposTransformer>> GetMerchantTiers(int merchantId, DateTime dateTimeUtc, MerchantTierStatusTypeEnum status, int? top = 0)
        {
            return await GetMerchantTiers(merchantId, dateTimeUtc, new MerchantTierStatusTypeEnum[] { status }, top);
        }
        /// <summary>
        /// Merchant Tiers with limited columns returned for eftpos transformer
        /// </summary>
        /// <param name="merchantId">Cashrewards merchantid</param>
        /// <param name="dateTimeUtc">time to check for a merchant tier by merchantId</param>
        /// <param name="statusList">statuss to check. Empty list for all</param>
        /// <param name="top">Only return this many matches</param>
        /// <returns>List of merchant tiers</returns>
        private async Task<IList<MerchantTierEftposTransformer>> GetMerchantTiers(int merchantId, DateTime dateTimeUtc, MerchantTierStatusTypeEnum[] statusList, int? top = 0)
        {
            string getActiveMerchantTierQuery = GetMerchantTierQuery(merchantId, dateTimeUtc, statusList, top);

            return (await _readOnlyRepository.Query<MerchantTierEftposTransformer>(getActiveMerchantTierQuery, new
            {
                TopParam = top,
                Status = statusList,
                MerchantTierId = merchantId,
                DateTimeUtc = dateTimeUtc,
            })).ToList();
        }

        /// <summary>
        /// Generate the GetMerchantTierQuery
        /// </summary>
        /// <param name="merchantId">cr merchantid</param>
        /// <param name="dateTimeUtc"></param>
        /// <param name="statusList"></param>
        /// <param name="top">return all or limit to this amount</param>
        /// <returns>A query string to get merchant tiers</returns>
        private string GetMerchantTierQuery(int merchantId, DateTime dateTimeUtc, MerchantTierStatusTypeEnum[] statusList, int? top = 0)
        {
            string topLimit = (top != null && top > 0) ? @"TOP (@TopParam) " : @"";
            string statusWhere = (statusList != null && statusList.Length > 0) ? @" AND merchantTier.[Status] IN @Status " : @"";
            string getActiveMerchantTierQuery = @"
                SELECT " + topLimit + @"
                    merchantTier.[MerchantTierId] AS MerchantTierId,
	                merchantTier.[TierSpecialTerms] AS TierSpecialTerms,
	                merchantTier.[TierCommTypeId] AS TierCommTypeId,
	                merchantTier.[Commission] AS Commission,
	                merchantTier.[TierReference] AS TierReference,
                    merchant.[CurrencyId] AS CurrencyId,
	                merchant.[MerchantId] AS MerchantId,
	                merchant.[ApprovalWaitDays] AS ApprovalWaitDays,
	                network.[NetworkId] AS NetworkId,
	                network.[NetworkKey] AS NetworkKey,
	                network.[GstStatusId] AS GstStatusId
                  FROM MerchantTier merchantTier 
                    LEFT JOIN Merchant merchant on merchantTier.[MerchantId] = merchant.[MerchantId] 
                    LEFT JOIN Network network on network.[NetworkId] = merchant.[NetworkId] 
                  WHERE merchantTier.[MerchantId] = @MerchantTierId
                  AND @DateTimeUtc BETWEEN merchantTier.[StartDateUtc] AND merchantTier.[EndDateUtc]
                " + statusWhere
                + @";";
            return getActiveMerchantTierQuery;
        }
    }
}