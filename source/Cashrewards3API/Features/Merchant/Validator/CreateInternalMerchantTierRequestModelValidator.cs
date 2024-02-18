using Cashrewards3API.Features.Merchant.Models;
using FluentValidation;

namespace Cashrewards3API.Features.Merchant
{
    public class CreateInternalMerchantTierRequestModelValidator:  AbstractValidator<CreateInternalMerchantTierRequestModel>
    {
        public CreateInternalMerchantTierRequestModelValidator()
        {
            RuleFor(x => x.TierName).NotEmpty();
            RuleFor(x => x.PromotionBonusValue).NotEmpty();
            RuleFor(x => x.PromotionBonusType).NotEmpty();
            RuleFor(x => x.PromotionDateMin).NotEmpty();
            RuleFor(x => x.PromotionDateMax).NotEmpty();
            RuleFor(x => x.TierDescription).NotEmpty();
            RuleFor(x => x.TierReference).NotEmpty();
            RuleFor(x => x.MerchantId).NotEmpty().GreaterThan(0);
        }
    }
}