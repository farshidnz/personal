using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Transaction
{
    public interface ITransactionService
    {
        Task<TransactionModel> CreateTransactionEventAsync(TransactionDto transactionDto);

        Task<int> GetMemberFirstPurchaseTransactionId(int memberId);
    }

    public class TransactionService : ITransactionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IReadOnlyRepository _readOnlyRepository;
        private readonly ILogger<TransactionService> _logger;
        private readonly IAmazonDynamoDB _amazonDynamoDb;
        private readonly IAmazonEventBridge _amazonEventBridge;
        private readonly string _eventBusName;

        public TransactionService(
            IConfiguration configuration,
            ILogger<TransactionService> logger,
            IAmazonDynamoDB amazonDynamoDb,
            IAmazonEventBridge amazonEventBridge,
            IHttpContextAccessor httpContextAccessor,
            IReadOnlyRepository readOnlyRepository)
        {
            _logger = logger;
            _amazonDynamoDb = amazonDynamoDb;
            _amazonEventBridge = amazonEventBridge;
            _httpContextAccessor = httpContextAccessor;
            _readOnlyRepository = readOnlyRepository;
            _eventBusName = configuration["Transaction:EventBusName"];
        }

        public async Task<TransactionModel> CreateTransactionEventAsync(TransactionDto transactionDto)
        {
            var transaction = await CreateTransactionMessageAsync(transactionDto);
            var putEventsRequest = new PutEventsRequest
            {
                Entries = new List<PutEventsRequestEntry>
                {
                    new PutEventsRequestEntry
                    {
                        EventBusName=_eventBusName,
                        Detail = JsonConvert.SerializeObject(transaction),
                        DetailType = "partner-transaction",
                        Source = "cashrewards-cr3-api"
                    }
                }
            };

            var response = await _amazonEventBridge.PutEventsAsync(putEventsRequest);
            _logger.LogInformation(JsonConvert.SerializeObject(response));

            return transaction;
        }

        public async Task<int> GetMemberFirstPurchaseTransactionId(int memberId)
        {
            const string query = @"
                SELECT Min (TransactionID)
                FROM dbo.[Transaction]
                WHERE MemberId = @MemberId
                AND TransactionTypeId IN @TransactionTypeIds";

            var firstPurchaseTransactionId = await _readOnlyRepository.QueryFirstOrDefault<int>(query, new
            {
                MemberId = memberId,
                TransactionTypeIds = new[]
                {
                    TransactionTypeEnum.Sale,
                    TransactionTypeEnum.CashbackClaim
                }
            });

            return firstPurchaseTransactionId;
        }

        private async Task<TransactionModel> CreateTransactionMessageAsync(TransactionDto transactionDto)
        {
            var cognitoAppClientId = ExtractCognitoAppClientId();
            var clientId = await GetPartner(cognitoAppClientId);
            var transaction = new TransactionModel
            {
                Type = transactionDto.Type,
                RefNum = transactionDto.RefNum,
                AuthMerchantId = transactionDto.AuthMerchantId,
                AcquirerICA = transactionDto.AcquirerICA,
                LocationId = transactionDto.LocationId,
                Timestamp = transactionDto.Timestamp,
                Amount = transactionDto.Amount,
                ClientId = clientId,
                MemberId = transactionDto.MemberId
            };

            return transaction;
        }

        private string ExtractCognitoAppClientId()
        {
            var accessToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var jwt = accessToken.ToString()?.Split(" ")[1];
            var token = new JwtSecurityToken(jwt);
            var cognitoAppClientId = token.Claims.First(c => c.Type == "client_id").Value;
            _logger.LogInformation($"client_id: {cognitoAppClientId}");

            return cognitoAppClientId;
        }

        private async Task<string> GetPartner(string cognitoAppClientId)
        {
            var context = new DynamoDBContext(_amazonDynamoDb);
            var partner = await context.LoadAsync<Partners>(cognitoAppClientId);
            _logger.LogInformation($"CR ClientId: {partner?.ClientId}");

            return partner?.ClientId;
        }
    }
}
