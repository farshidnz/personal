using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Enum
{
    public enum ClientVerificationType
    {
        [Description("Pending")]
        Pending = 101,
        [Description("Matched")]
        Matched = 102,
        [Description("Not Matched")]
        NotMatched = 103,
        [Description("Not Applicable")]
        NotApplicable = 104
    }
}
