using Cashrewards3API.Features.Offers;
using System.Collections.Generic;

namespace Cashrewards3API.Common.Utils
{
    public class OfferComparer : IEqualityComparer<OfferViewModel>
    {
        public bool Equals(OfferViewModel x, OfferViewModel y)
        {
            return (x.OfferId == y.OfferId && x.ClientId == y.ClientId);
        }

        public int GetHashCode(OfferViewModel obj)
        {
            return 0;
        }
    }
}