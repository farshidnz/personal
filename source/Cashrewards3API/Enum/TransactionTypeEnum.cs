using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Cashrewards3API.Enum
{
    public enum TransactionTypeEnum
    {
        [Description("PromoReferAMate")]
        [EnumMember]
        PromoReferAMate = 1,
        [Description("PromoReferAMateReferrer")]
        [EnumMember]
        PromoReferAMateReferrer = 2,
        [Description("PromoReactivationOffer")]
        [EnumMember]
        PromoReactivationOffer = 3,
        [Description("PromoSignupBonus")]
        [EnumMember]
        PromoSignupBonus = 4,
        [Description("Sale")]
        [EnumMember]
        Sale = 5,
        [Description("CashbackClaim")]
        [EnumMember]
        CashbackClaim = 6,
        [Description("PromotionSale")]
        [EnumMember]
        PromotionSale = 7,
        [Description("AccountCorrection")]
        [EnumMember]
        AccountCorrection = 8,
        [Description("PromoMS")]
        [EnumMember]
        PromoMs = 9
    }
}
