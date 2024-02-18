using Cashrewards3API.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Offers
{
    public static class OfferHelpers
    {
        public static IEnumerable<OfferViewModel> ExcludePremiumDisabledMerchantOffers(IEnumerable<OfferViewModel> offers) =>
            offers.Where(offer => !offer.MerchantIsPremiumDisabled.HasValue || !offer.MerchantIsPremiumDisabled.Value);

        public static IEnumerable<T> ExcludePausedMerchantOffers<T>(IEnumerable<T> offers) where T : IHaveIsMerchantPaused =>
            offers.Where(offer => !offer.IsMerchantPaused);
    }
}
