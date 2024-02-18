using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Enum
{
    public enum MobileAppTrackingTypeEnum
    {
        [Description("ExternalBrowser")]
        ExternalBrowser = 1,

        [Description("InAppBrowser")]
        InAppBrowser = 2,

        [Description("AppToApp")]
        AppToApp = 3
    }
}
