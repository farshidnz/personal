using Cashrewards3API.Common.Model;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Offers;
using Cashrewards3API.Features.Promotion.Model;
using System.Collections.Generic;

namespace Cashrewards3API.FeaturesToggle
{
    public interface IPausedMerchantFeatureToggle
    {
        bool IsFeatureEnabled { get; set; }
        IEnumerable<T> ExcludeItemsWhenIsPaused<T>(IEnumerable<T> items) where T : IHaveIsPaused;
        IEnumerable<T> ExcludeItemsWhenIsMerchantPaused<T>(IEnumerable<T> items) where T : IHaveIsMerchantPaused;
        PromotionCategoryInfo ExcludeCategoryItemsWhenIsPaused(PromotionCategoryInfo category, IEnumerable<MerchantViewModel> merchants, IEnumerable<OfferDto> offers);
    }
}
