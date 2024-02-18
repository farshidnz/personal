using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Cashrewards3API.Features.Merchant.Models.OfflineMerchantStore;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class InStoreMerchantModel : CardLinkedMerchantModel
    {
        public List<Store> Stores { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Nullable<decimal> MinimumSpend { get; set; }

    }
}
