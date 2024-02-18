using Cashrewards3API.Features.Category;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Features.Promotion.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Promotion
{
    public interface IMemberBonusService
    {
        Task<MemberBonusDto> GetMemberBonus(int clientId, string code);
    }

    public class MemberBonusService : IMemberBonusService
    {
        private const string _gstExcludeFlag = "accepted";

        private readonly IPromoAppService _promoAppService;
        private readonly IMerchantService _merchantService;
        private readonly ICategoryService _categoryService;

        public MemberBonusService(
            IPromoAppService promoAppService,
            IMerchantService merchantService,
            ICategoryService categoryService)
        {
            _promoAppService = promoAppService;
            _merchantService = merchantService;
            _categoryService = categoryService;
        }

        public async Task<MemberBonusDto> GetMemberBonus(int clientId, string code)
        {
            var promoDetails = await _promoAppService.GetPromotionDetails(code);

            var promotion = promoDetails?.promotion;
            if (promoDetails == null || promotion == null || promoDetails.code != "200" || !promoDetails.valid)
            {
                return new MemberBonusDto()
                {
                    Bonus = 0,
                    MinSpend = 0,
                    PurchaseWindow = 0
                };
            }

            return new MemberBonusDto()
            {
                Bonus = decimal.Parse(promotion.bonus_value),
                MinSpend = promotion.rules?.sale_value?.required == true ? decimal.Parse(promotion.rules.sale_value.min ?? "0") : 0,
                PurchaseWindow = promotion.rules?.first_purchase_window?.required == true ? int.Parse(promotion.rules.first_purchase_window.max ?? "0") : 0,
                TermsAndConditions = promoDetails?.terms_and_condition,
                ExcludeGST = !(promotion?.rules?.gst?.Contains(_gstExcludeFlag) ?? false),
                Merchant = await GetMerchantBonusRule(clientId, promotion?.rules?.merchant_id),
                StoreType = GetStoreTypeBonusRule(promotion?.rules?.store_type),
                Category = await GetCategoryBonusRule(clientId, promotion?.rules?.category_id)
            };

        }

        private async Task<MemberBonusRule> GetMerchantBonusRule(int clientId, PromoDetailsModel.AdditionalRule rule)
        {
            if (rule?.required != true)
            {
                return null;
            }

            IEnumerable<MerchantViewModel> includeMerchants = null;
            IEnumerable<MerchantViewModel> excludeMerchants = null;

            if (rule.@in != null)
            {
                includeMerchants = await _merchantService.GetMerchantsForStandardClient(clientId, rule.@in.Split(',').Select(id => int.Parse(id)));
            }

            if (rule.not_in != null)
            {
                excludeMerchants = await _merchantService.GetMerchantsForStandardClient(clientId, rule.not_in.Split(',').Select(id => int.Parse(id)));
            }

            return new MemberBonusRule()
            {
                In = includeMerchants?.Select(m => m.MerchantName),
                NotIn = excludeMerchants?.Select(m => m.MerchantName)
            };
        }

        private static MemberBonusRule GetStoreTypeBonusRule(PromoDetailsModel.AdditionalRule promoAdditionalRule)
        {
            if (promoAdditionalRule?.required != true)
            {
                return null;
            }

            return new MemberBonusRule()
            {
                In = promoAdditionalRule.@in?.Split(','),
                NotIn = promoAdditionalRule.not_in?.Split(',')
            };
        }

        private async Task<MemberBonusRule> GetCategoryBonusRule(int clientId, PromoDetailsModel.AdditionalRule rule)
        {
            if (rule?.required != true)
            {
                return null;
            }

            IEnumerable<CategoryDto> includeCategories = null;
            IEnumerable<CategoryDto> excludeCategories = null;

            var rootCategories = await _categoryService.GetRootCategoriesAsync(clientId, null, Status.All);

            if (rule.@in != null)
            {
                includeCategories = rootCategories.Where(c => rule.@in.Split(',').Select(id => int.Parse(id)).Contains(c.Id));
            }

            if (rule.not_in != null)
            {
                excludeCategories = rootCategories.Where(c => rule.not_in.Split(',').Select(id => int.Parse(id)).Contains(c.Id));
            }

            return new MemberBonusRule()
            {
                In = includeCategories?.Select(c => c.Name),
                NotIn = excludeCategories?.Select(c => c.Name)
            };
        }
    }
}
