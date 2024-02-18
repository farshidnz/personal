namespace Cashrewards3API.Features.Feeds.Models
{
    public class MerchantFeedTierDataModel
    {
        public int MerchantId { get; set; }
        public int MerchantTierId { get; set; }
        public decimal Commission { get; set; }
        public decimal ClientComm { get; set; }
        public decimal MemberComm { get; set; }
        public int TierCommTypeId { get; set; }
        public string TierDescription { get; set; }
        public int ClientId { get; set; }
    }
}
