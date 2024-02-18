using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Cashrewards3API.Common.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Common
{
    public interface IClientService
    {
        Task<string> GetPartner(string cognitoAppClientId);
    }
    public class ClientService : IClientService
    {
        private readonly IAmazonDynamoDB _amazonDynamoDb;

        public ClientService(IAmazonDynamoDB amazonDynamoDb)
        {
            _amazonDynamoDb = amazonDynamoDb;
        }

        public async Task<string> GetPartner(string cognitoAppClientId)
        {
            var context = new DynamoDBContext(_amazonDynamoDb);
            var partner = await context.LoadAsync<Partners>(cognitoAppClientId);

            return partner?.ClientId;
        }

    }
}
