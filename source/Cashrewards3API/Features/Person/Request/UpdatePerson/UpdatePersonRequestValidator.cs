using FluentValidation;

namespace Cashrewards3API.Features.Person.Request.UpdatePerson
{
    public class UpdatePersonRequestValidator : AbstractValidator<UpdatePersonRequest>
    {
        public UpdatePersonRequestValidator()
        {
            RuleFor(val => val.PremiumStatus).IsInEnum();
        }
    }
}