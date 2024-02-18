using Cashrewards3API.Features.MemberClick;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3API.Tests.Features.MemberClick.Utils
{
    public class MemberClickUtils
    {
        public static List<OfferModel> GetOfferModelsTestData(List<OfferModelData> data,string query, object parameters)
        {
            var p = JsonConvert.DeserializeObject<QueryParams>(JsonConvert.SerializeObject(parameters));
            return data
                .Where(o => p.ClientIds.Contains(o.ClientId) || !query.Contains("ClientId IN @ClientIds"))
                .Where(o => p.HyphenatedString == o.HyphenatedString || !query.Contains("HyphenatedString = @HyphenatedString"))
                .Select(o => o as OfferModel)
                .ToList();
        }

        public static List<MerchantModel> GetMerchantModelsTestData(List<MerchantModelData> data,string query, object parameters)
        {
            var p = JsonConvert.DeserializeObject<QueryParams>(JsonConvert.SerializeObject(parameters));
            return data
                .Where(m => p.ClientId == m.ClientId || !query.Contains("ClientId = @ClientId"))
                .Where(m => p.HyphenatedString == m.HyphenatedString || !query.Contains("HyphenatedString = @HyphenatedString"))
                .Where(m => !p.InStoreNetworkIds?.Contains(m.NetworkId) ?? true || !query.Contains("NetworkId NOT IN @InStoreNetworkIds"))
                .Where(m => !p.InStoreNetworks?.Contains(m.NetworkId) ?? true || !query.Contains("NetworkId NOT IN @InStoreNetworks"))
                .Select(m => m as MerchantModel)
                .ToList();
        }
    }

    public class QueryParams
    {
        public int ClientId { get; set; }
        public List<int> ClientIds { get; set; }
        public string HyphenatedString { get; set; }
        public List<int> InStoreNetworkIds { get; set; }
        public List<int> InStoreNetworks { get; set; }
    }

    public class MerchantModelData : MerchantModel
    {
        public string HyphenatedString { get; set; }
    }

    public class OfferModelData : OfferModel
    {
        public string HyphenatedString { get; set; }
    }
}
