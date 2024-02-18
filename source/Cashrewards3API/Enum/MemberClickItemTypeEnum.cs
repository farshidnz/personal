using System.ComponentModel;

namespace Cashrewards3API.Enum
{
    public enum MemberClickItemTypeEnum
    {
        [Description("Deal")]
        Deal,
        [Description("Merchant")]
        Merchant,
        [Description("Merchant Alias")]
        MerchantAlias,
        [Description("Merchant Tier")]
        MerchantTier,
        [Description("Merchant Tier Link")]
        MerchantTierLink,
        [Description("Offer")]
        Offer,
        [Description("Product")]
        Product
    }
}
