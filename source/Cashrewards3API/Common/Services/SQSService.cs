using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon.SQS.Model;
using Cashrewards3API.Common.Configuration;
using Cashrewards3API.Common.Events;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Enum;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Services
{
    public class SQSService : IMessage
    {
        private AmazonSQSClient _client;

        private AmazonSQSClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new AmazonSQSClient();
                }
                return _client;
            }
        }

        private readonly AWSInfrastructureSettings _awsSettings;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private const string MessageStructure = "Raw";

        public SQSService(IAmazonSimpleNotificationService snsClient, AWSInfrastructureSettings awsSettings)
        {
            _snsClient = snsClient;
            _awsSettings = awsSettings;
        }


        /// <summary>
        /// Updateds the premium member property.
        /// </summary>
        /// <param name="message">The message.</param>
        public async Task UpdatedPremiumMemberProperty(MemberPremiumUpdateProperty message)
        {
            await WriteMessageAsync(message, _awsSettings.SQS.UpdateLeanplumMemberUrl);
        }

        private async Task WriteMessageAsync(object message, string queueUrl, string messageGroupId = null)
        {
            SendMessageRequest request = BuildSendMessageRequest(message, queueUrl, messageGroupId);
            await Client.SendMessageAsync(request);
        }

        private SendMessageRequest BuildSendMessageRequest(object message, string queueUrl, string messageGroupId = null)
        {
            var messageType = message.GetType().FullName;

            var messageBody = JsonConvert.SerializeObject(message, new JsonSerializerSettings() { ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() } });
            var request = new SendMessageRequest
            {
                MessageBody = messageBody,
                QueueUrl = queueUrl,

                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    {
                        "MessageType",
                        new MessageAttributeValue {DataType = "String", StringValue = messageType}
                    },
                    {
                        "CorrelationId",
                        new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = Trace.CorrelationManager.ActivityId.ToString()
                        }
                    }
                }
            };

            if (!string.IsNullOrEmpty(messageGroupId))
            {
                request.MessageGroupId = messageGroupId;
            };

            return request;
        }

        public async Task UpdatePremiumMemberEvent(MemberPremiumUpdateEvent message, PremiumStatusEnum premiumStatus)
        {
            message.Event.Params.Add("PremiumStatus", ((int)premiumStatus).ToString());
            await WriteMessageAsync(message, _awsSettings.SQS.UpdateMemberLeanplumEventUrl);
        }

        public async Task MemberFirstClickEvent(MemberFirstClickEvent message)
        {
            await WriteMessageAsync(message, _awsSettings.SQS.MemberFirstClickEventUrl);
        }
    }
}