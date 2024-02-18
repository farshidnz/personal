using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Cashrewards3API.Enum
{
    public enum TierTypeEnum
    {
        [Description("Discount")]
        [EnumMember]
        Discount = 117,
        [Description("MaxDiscount")]
        [EnumMember]
        MaxDiscount = 121,
        [Description("Hidden")]
        [EnumMember]
        Hidden= 125
    }
}
