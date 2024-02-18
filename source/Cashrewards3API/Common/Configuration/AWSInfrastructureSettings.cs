namespace Cashrewards3API.Common.Configuration
{
    public class AWSInfrastructureSettings
    {
        public string Region { get; set; }
        public string UserPoolClientId { get; set; }
        public string UserPoolId { get; set; }
        public string MainSiteApiKeyName { get; set; }
        public SNS SNS { get; set; }
        public SQS SQS { get; set; }
    }

    public class SNS
    {
        public string ClickCreateTopicArn { get; set; }
        
    }

    public class SQS
    {
        public string UpdateLeanplumMemberUrl { get; set; }
        public string UpdateMemberLeanplumEventUrl { get; set; }
        public string MemberFirstClickEventUrl { get; set; }
    }
}