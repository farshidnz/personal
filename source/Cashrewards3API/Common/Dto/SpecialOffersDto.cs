using Cashrewards3API.Features.Offers;
using System.Collections.Generic;

namespace Cashrewards3API.Common.Dto
{
    public class SpecialOffersDto
    {
        public IEnumerable<OfferDto> PremiumFeatureOffers { get; set; }
        public IEnumerable<OfferDto> CashBackIncreasedOffers { get; set; }
    }
}