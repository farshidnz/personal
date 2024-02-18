using System;
using FluentValidation;

namespace Cashrewards3API.Features.Member.Request
{
    public class CreateCognitoMemberRequestValidator : AbstractValidator<CreateCognitoMemberRequest>
    {
        public CreateCognitoMemberRequestValidator()
        {
            RuleFor(val => val.CognitoId).NotEmpty();
            RuleFor(val => val.Email).EmailAddress().MaximumLength(200);
            RuleFor(val => val.FirstName).MaximumLength(50);
            RuleFor(val => val.LastName).MaximumLength(50);
            RuleFor(val => val.OriginalSource).MaximumLength(50);
            RuleFor(val => val.ClientId).IsInEnum();
            RuleFor(val => val.FacebookUsername).Custom((facebookUserName, context) =>
            {
                var model = ((CreateCognitoMemberRequest)context.InstanceToValidate);

                if (!string.IsNullOrEmpty(facebookUserName) &&
                    !facebookUserName.Equals(model.Email, StringComparison.InvariantCultureIgnoreCase))
                {
                    context.AddFailure("Invalid Facebook user name");
                }
                
            });
        }
    }
}