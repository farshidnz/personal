using Cashrewards3API.Common.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class CardLinkedMerchantModel : MerchantModel
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public VisaMetaData VisaMeta { get; set; }

    }
}
