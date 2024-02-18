using Cashrewards3API.Common.Model;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Offers;
using Cashrewards3API.Features.Promotion.Model;
using System.Collections.Generic;
using System.Linq;

namespace Cashrewards3API.FeaturesToggle
{
    public class PausedMerchantFeatureToggle : IPausedMerchantFeatureToggle
    {
        public bool IsFeatureEnabled { get; set; }
        public PausedMerchantFeatureToggle(IFeatureToggle featureToggle)
        {
            IsFeatureEnabled = featureToggle.IsEnabled(Common.FeatureFlags.IS_MERCHANT_PAUSED);
        }
        public IEnumerable<T> ExcludeItemsWhenIsPaused<T>(IEnumerable<T> items) where T : IHaveIsPaused
        {
            return IsFeatureEnabled ?
                items.Where(i => !i.IsPaused) : items;
        }
        public IEnumerable<T> ExcludeItemsWhenIsMerchantPaused<T>(IEnumerable<T> items) where T : IHaveIsMerchantPaused
        {
            return IsFeatureEnabled ?
                items.Where(i => !i.IsMerchantPaused) : items;
        }
        public PromotionCategoryInfo ExcludeCategoryItemsWhenIsPaused(PromotionCategoryInfo category, IEnumerable<MerchantViewModel> merchants, IEnumerable<OfferDto> offers)
        {
            if (IsFeatureEnabled && category is not null)
            {
                category.Merchants = category.Merchants?.Where(merchant => merchants.Any(m => m.MerchantId == merchant.MerchantId)).ToList();
                category.Items = category.Items?.Where(item =>
                {
                    return item.ItemType == (int)PromotionCategoryItemTypeEnum.Merchant ?
                         merchants.Any(merchant => merchant.MerchantId == item.ItemId)
                            : offers.Any(offer => offer.Id == item.ItemId);
                }).ToList();
            }
            return category;
        }
    }
}
