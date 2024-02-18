using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.ReferAFriend.Model
{
    public class RafRules
    {
        public RafAccessCode AccessCode { get; set; }
        public RafPurchaseWindow PurchaseWindow { get; set; }
        public RafSaleValue SaleValue { get; set; }

    }
}
