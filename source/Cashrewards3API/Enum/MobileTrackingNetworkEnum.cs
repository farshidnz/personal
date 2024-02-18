using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Enum
{

    public enum MobileTrackingNetworkEnum
    {
        [Description("Button")]
        Button = 1,

        [Description("OtherProvider")]
        OtherProvider = 2,

        [Description("Special")]
        Special = 3,

        [Description("Deeplink")]
        Deeplink = 4
    }
}
