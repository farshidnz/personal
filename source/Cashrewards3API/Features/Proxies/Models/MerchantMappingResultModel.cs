using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Proxies.Models
{
    public class MerchantMappingResultModel
    {
        public int StatusCode { get; set; }
        public MerechantModel MerchantMap { get; set; }
    }

    public class MerechantModel
    {
        public int CrMerchantId { get; set; }
        public string McLocationId { get; set; }
        public string McAuthMerchantId { get; set; }
        public string McAuthAcquirerIca { get; set; }
        public string McMerchantDbaName { get; set; }
        public string McMerchantAddress { get; set; }
        public string McMerchantCity { get; set; }
        public string McMerchantState { get; set; }
    }
}
