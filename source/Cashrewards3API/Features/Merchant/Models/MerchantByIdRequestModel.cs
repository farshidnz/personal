using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantByIdRequestModel
    {
        public int ClientId { get; set; }
        public int MerchnantId { get; set; }
    }
}
