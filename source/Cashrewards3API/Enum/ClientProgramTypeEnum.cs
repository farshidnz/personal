using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Cashrewards3API.Enum
{
    public enum ClientProgramTypeEnum
    {
        [Description("CashProgram")]
        [EnumMember]
        CashProgram = 100,

        [Description("ProductProgram")]
        [EnumMember]
        ProductProgram = 101,

        [Description("PointsProgram")]
        [EnumMember]
        PointsProgram = 102,
    }
}
