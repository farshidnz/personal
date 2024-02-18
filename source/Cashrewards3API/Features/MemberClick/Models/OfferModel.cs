namespace Cashrewards3API.Features.MemberClick
{
    public class OfferModel
    {
        public int OfferId { get; set; }

        public string MerchantHyphenatedString { get; set; }

        public string TrackingLink { get; set; }

        public string MerchantTrackingLink { get; set; }

        public string RegularImageUrl { get; set; }

        public int ClientId { get; set; }

        public bool IsMerchantPaused { get; set; }
    }
}
