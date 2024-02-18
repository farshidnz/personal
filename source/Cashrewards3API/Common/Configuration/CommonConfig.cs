namespace Cashrewards3API.Common
{
    public class CommonConfig
    {
        public string InStoreNetworkId { get; set; }
        public string ClickCreateTopicArn { get; set; }
        public Transaction Transaction { get; set; }

        public Promotion Promotion { get; set; }
        public GstConfig GstConfig { get; set; }
        public string MemberCreateTopicArn { get; set; }
        public Talkable Talkable { get; init; }

        public TrueRewards TrueRewards { get; init; }
        public StrapiConfig Strapi { get; init; }
    }

    public class Transaction
    {
        public int CashrewardsReferAMateMerchantId { get; set; }

        public int CashrewardsLegacyBonusMerchantId { get; set; }
        public int CashrewardsBonusMerchantId { get; set; }

        public int CashrewardsActivationBonusMerchantId { get; set; }

        public string GiftCardMerchantIds { get; set; }
        public int ShopGoNetworkId { get; set; }
        public int CashrewardsClientId { get; set; }
        public int GstStatusExclusiveGstId { get; set; }
        public int TransactionStatusPendingId { get; set; }
        public int CurrencyAudId { get; set; }
        public int StatusPendingId { get; set; }
        public int NetworkTranStatusPendingId { get; set; }
    }

    public class Promotion
    {
        public int TierTypePromotionId { get; set; }
    }

    public class CacheConfig
    {
        public int CategoryDataExpiry { get; set; }
        public int OfferDataExpiry { get; set; }
        public int CardLinkedMerchantDataExpiry { get; set; }
        public int MerchantDataExpiry { get; set; }
        public int CrApplicationKeyExpiry { get; set; }
        public int EarlyCacheRefreshPercentage { get; set; }
    }

    public class GstConfig
    {
        public decimal Percentage { get; set; }
        public decimal Adjustment { get; set; }
    }

    public class Talkable
    {
        public string ApiBaseAddress { get; set; }
        public string Environment { get; set; }
        public string ApiKey { get; set; }
    }

    public class TrueRewards
    {
        public string TokenIssuer { get; set; }
        public string App { get; set; }
        public string ApiKey { get; set; }
    }

    public class StrapiConfig
    {
        public string ApiBaseAddress { get; set; }
        public string ApiBaseAddressV4 { get; set; }
    }
}
