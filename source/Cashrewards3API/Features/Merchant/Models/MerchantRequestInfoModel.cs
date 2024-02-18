using Cashrewards3API.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantRequestInfoModel
    {
        public int ClientId { get; set; }
        public int? PremiumClientId { get; set; }
        public int CategoryId { get; set; }
        public MerchantInstoreFilterEnum InStoreFlag { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }

    }
}
