using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Cashrewards3API.Common.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services
{
    public class SNSService
    {
        private readonly AWSInfrastructureSettings _awsSettings;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private const string MessageStructure = "Raw";

        public SNSService(IAmazonSimpleNotificationService snsClient, AWSInfrastructureSettings awsSettings)
        {
            _snsClient = snsClient;
            _awsSettings = awsSettings;
        }

        private async Task SendEvent(string message, string topic)
        {
            var request = new PublishRequest
            {
                Message = message,
                TopicArn = topic,
                MessageStructure = "Raw",
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "MessageType",
                        new MessageAttributeValue {DataType = "String", StringValue = message.GetType().FullName}
                    }
                }
            };

            await _snsClient.PublishAsync(request);
        }
    }
}