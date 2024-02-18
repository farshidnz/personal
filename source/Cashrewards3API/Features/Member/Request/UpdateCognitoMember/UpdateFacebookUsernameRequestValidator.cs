using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashrewards3API.Features.Member.Interface;
using FluentValidation;

namespace Cashrewards3API.Features.Member.Request.UpdateCognitoMember
{
    public class UpdateFacebookUsernameRequestValidator : AbstractValidator<UpdateFacebookUsernameRequest>
    {
        
        public UpdateFacebookUsernameRequestValidator()
        {   
            RuleFor(val => val.FacebookUsername).NotNull().EmailAddress();
        }
    }
}
