using Cashrewards3API.Common;
using Cashrewards3API.Features.Member.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Member
{
    [Route("api/v1")]
    [Authorize]
    public class MemberController : BaseController
    {
        private readonly IMemberRegistrationService _memberRegistrationService;
        private readonly IMemberService _memberService;
        private readonly IRequestContext _requestContext;

        public MemberController(IMemberRegistrationService memberRegistrationService, IMemberService memberService, IRequestContext requestContext)
        {
            _memberRegistrationService = memberRegistrationService;
            _memberService = memberService;
            _requestContext = requestContext;
        }

        [HttpPost]
        [Route("member/register-blue")]
        [Authorize(Policy = "CR-ClientCredentials")]
        [Authorize(Policy = "CR-AccessToken")]
        public async Task<IActionResult> RegisterBlueMember()
        {
            if (String.IsNullOrEmpty(UserToken.CognitoId))
                throw new Exceptions.BadRequestException("Cognito Id not Valid");

            return Ok(await _memberRegistrationService.CreateBlueMemberFromCashRewardsMember(UserToken.CognitoId));
        }

        [HttpGet]        
        [Route("member/trauthtoken")]
        public async Task<IActionResult> GetTRAuthToken()
        {
            var member = _requestContext.Member;

            string token = await _memberService.GetTRAuthToken(member);

            return Ok(token);
        }
    }
}