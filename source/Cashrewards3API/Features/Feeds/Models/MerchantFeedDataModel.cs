namespace Cashrewards3API.Features.Feeds.Models
{
    public class MerchantFeedDataModel
    {
        public int MerchantId { get; set; }
        public string MerchantName { get; set; }
        public string DescriptionShort { get; set; }
        public string HyphenatedString { get; set; }
        public string WebsiteUrl { get; set; }
        public int ClientId { get; set; }
    }
}
