using Amazon.DynamoDBv2.DataModel;

namespace Cashrewards3API.Common.Dto
{
    [DynamoDBTable("partners")]
    public class Partners
    {
        [DynamoDBHashKey("cognito_app_client_id")] //Partition key
        public string CognitoAppClientId
        {
            get; set;
        }

        [DynamoDBProperty("client_id")]
        public string ClientId
        {
            get; set;
        }

        [DynamoDBProperty("partner_name")]
        public string PartnerName
        {
            get; set;
        }

    }
}