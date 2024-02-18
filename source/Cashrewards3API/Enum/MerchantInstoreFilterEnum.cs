using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Enum
{
    public enum MerchantInstoreFilterEnum
    {
        [Description("InStore")]
        InStore = 1,

        [Description("Online")]
        Online = 2,

        [Description("All")]
        All = 3
    }
}
