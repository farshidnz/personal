using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantRequestModel
    {
        public int ClientId { get; set; }
        public string Filter { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
    }
}
