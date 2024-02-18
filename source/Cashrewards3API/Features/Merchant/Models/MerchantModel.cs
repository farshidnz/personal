using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant.Models
{
    public class MerchantModel : MerchantBasicModel
    {
        public IList<OfferModel> Offers { get; set; }

        public IList<MerchantTierResultModel> MerchantTiers { get; set; }
    }
}
