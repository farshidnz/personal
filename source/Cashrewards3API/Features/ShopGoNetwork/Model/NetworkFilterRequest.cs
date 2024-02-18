using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.ShopGoNetwork.Model
{
    public class NetworkFilterRequest
    {
        public int Skip { get; set; } = 0;
        public int Limit { get; set; } = 20;
    }
}
