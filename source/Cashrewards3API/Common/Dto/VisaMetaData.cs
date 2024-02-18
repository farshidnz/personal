using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Common.Dto
{
    public class VisaMetaData
    {
        public string OfferId { get; set; }
        public string MerchantGroupName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? OfferExpiry { get; set; }

    }
}
