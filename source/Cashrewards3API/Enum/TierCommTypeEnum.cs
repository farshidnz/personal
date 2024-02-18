using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Cashrewards3API.Enum
{
    public enum TierCommTypeEnum
    {
        [Description("Percentage")]
        [EnumMember]
        Percentage = 101,
        [Description("Dollar")]
        [EnumMember]
        Dollar = 100
    }
}
