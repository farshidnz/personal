using Cashrewards3API.Internals.BonusTransaction.Models;
using FluentValidation;

namespace Cashrewards3API.Internals.BonusTransaction
{
    public class CreateBonusTransactionRequestModelValidator : AbstractValidator<CreateBonusTransactionRequestModel>
    {
        public CreateBonusTransactionRequestModelValidator()
        {
            RuleFor(x => x.MemberId).NotEmpty();
            RuleFor(x => x.TransactionDisplayId).NotEmpty();
            RuleFor(x => x.TierName).NotEmpty();
            RuleFor(x => x.TierReference).NotEmpty();
            RuleFor(x => x.TierDescription).NotEmpty();
            RuleFor(x => x.MerchantId).NotEmpty();
            RuleFor(x => x.MerchantTierId).NotEmpty();
            RuleFor(x => x.Promotion).NotEmpty();
            RuleFor(x => x.Promotion.PromotionDateMin).NotEmpty();
            RuleFor(x => x.Promotion.PromotionDateMax).NotEmpty();
            RuleFor(x => x.Promotion.BonusType).NotEmpty();
            RuleFor(x => x.Promotion.BonusValue).NotEmpty();
        }
    }
}