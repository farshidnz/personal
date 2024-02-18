using System.Collections.Generic;
using Cashrewards3API.Common;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantByFilterRequestModel
    {
        public string Name { get; set; } = "";

        public List<int> Ids { get; set; } = new List<int>();

        public int Offset { get; set; } = 0;

        public int Limit { get; set; } = 20;

        public int ClientId => Constants.Clients.CashRewards;
    }
}