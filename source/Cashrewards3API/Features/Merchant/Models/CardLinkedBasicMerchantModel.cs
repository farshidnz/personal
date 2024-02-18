using Cashrewards3API.Common.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class CardLinkedBasicMerchantModel : MerchantBasicModel
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public VisaMetaData VisaMeta { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? MinimumSpend { get; set; }
    }
}
