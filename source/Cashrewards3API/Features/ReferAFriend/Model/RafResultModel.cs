using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashrewards3API.Enum;

namespace Cashrewards3API.Features.ReferAFriend.Model
{
    public class RafResultModel
    {
        public bool HasQualifiedTransactions { get; set; }
        public bool Status { get; set; }
        public RafPromotion Promotion { get; set; }
        
    }
}
