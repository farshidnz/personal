using Cashrewards3API.Common.Model;
using Cashrewards3API.Features.Merchant.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cashrewards3API.Features.Merchant
{
    public static class MerchantHelpers
    {
        public static IEnumerable<T> ExcludePremiumDisabledMerchants<T>(IEnumerable<T> merchants) where T : IHavePremiumDisabled =>
            merchants.Where(merchant => !merchant.IsPremiumDisabled.HasValue || !merchant.IsPremiumDisabled.Value);

        public static IEnumerable<T> ExcludePausedMerchants<T>(IEnumerable<T> merchants) where T : IHaveIsPaused =>
            merchants.Where(merchant => !merchant.IsPaused);

        public static MerchantStore ForceNoCommissionWhenPausedMerchant(MerchantStore merchantStore)
        {
            if (merchantStore.IsPaused)
            {
                merchantStore.ClientCommission = 0; merchantStore.ClientCommissionSummary = String.Empty;
                merchantStore.Tiers = new List<MerchantStore.Tier>();
            }
            return merchantStore;
        }
    }
}
