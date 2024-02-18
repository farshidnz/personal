using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services.Interfaces;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Member.Dto;
using Cashrewards3API.Features.Member.Interface;
using Cashrewards3API.Features.Member.Model;
using Cashrewards3API.Features.Member.Models;
using Cashrewards3API.Features.Member.Request;
using Cashrewards3API.Features.Member.Request.SignInMember;
using Cashrewards3API.Features.Member.Request.UpdateCognitoMember;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Member
{
    [ApiController]
    [Route("api/v1/internal")]
    [Authorize(Policy = "InternalPolicy")]
    public class MemberInternalController : BaseController
    {
        private readonly IMemberService _memberService;
        private readonly IMapper _mapper;
        private readonly IFeatureToggle _featureToggle;
        private const string RegexEmailWhiteList = @"^qa\+signup.*@cashrewards\.com$";
        public MemberInternalController(IMemberService memberService,
            IMapper mapper, IFeatureToggle featureToggle)
        {
            _mapper = mapper;
            _featureToggle = featureToggle;
            _memberService = memberService;
        }

        [HttpGet("memberinfo")]
        public async Task<IActionResult> GetMemberInfoAsync(int? clientId, string cognitoId)
            => clientId == null || cognitoId == null
                ? (IActionResult)BadRequest()
                : Ok(_mapper.Map<MemberDto>(await _memberService.GetMemberByCognitoId(clientId.GetValueOrDefault(), cognitoId)));

        [HttpGet("associated-members")]
        [ProducesResponseType(typeof(IEnumerable<MemberDto>), 200)]
        public async Task<IEnumerable<MemberDto>> GetAssocicatedMembersAsync(int memberId)
        {
            return await _memberService.GetAssocicatedMembersByMemberIdAsync(memberId);
        }

        /// <summary>
        /// Gets a member by id. If not found a 404 response
        /// is returned.
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>

        [HttpGet]
        [Route("member/{memberId:int}")]
        [ProducesResponseType(typeof(MemberDto), 200)]
        public async Task<IActionResult> GetMemberById(int memberId)
        {
            var member = await _memberService.GetMemberById(memberId);
            return member == null ? (IActionResult)NotFound($"memberId {memberId} does not exist") : Ok(member);
        }

        [HttpPost]
        [Route("member/signin")]
        [ProducesResponseType(typeof(SignInMemberInternalResponse), 200)]
        public async Task<SignInMemberInternalResponse> SignInMember([FromBody] SignInMemberInternalRequest request) =>
            await _memberService.SignInMember(request);

        [HttpGet]
        [Route("member/{memberId:int}/eftpostransformer")]
        [ProducesResponseType(typeof(MemberEftposTransformer), 200)]
        public async Task<IActionResult> GetMemberByIdForEftposTransformer(int memberId)
        {
            var member = await _memberService.GetMemberByIdForEftposTransformer(memberId);
            return member == null ? (IActionResult)NotFound($"memberId {memberId} does not exist") : Ok(member);
        }

        [HttpPost]
        [Route("member/RegisterCognitoMember")]
        [ProducesResponseType(typeof(CognitoMemberDto), 201)]
        public async Task<IActionResult> RegisterCognitoMember([FromBody] CreateCognitoMemberRequest request)
        {
            request.ClientId = request.ClientId == ClientsEnum.NA ? ClientsEnum.CashRewards : request.ClientId;

            var memberRequest = Mapper.Map<MemberModel>(request);
            memberRequest.IsValidated = false;
            if (_featureToggle.IsEnabled(FeatureFlags.WHITE_LIST_TEST_MEMBERS))
            {
                Regex reg = new Regex(RegexEmailWhiteList);
                memberRequest.IsValidated = reg.IsMatch(request.Email.ToLower());
            }

            MemberModel member = await _memberService.CreateCognitoMember(memberRequest);
            return Ok(Mapper.Map<CognitoMemberDto>(member));
        }

        [HttpPatch]
        [Route("member/facebookusername")]
        [ProducesResponseType(typeof(UpdateFacebookUsernameDto), 201)]
        public async Task<IActionResult> UpdateFacebookUsername([FromBody] UpdateFacebookUsernameRequest request)
        {
            return Ok(await _memberService.UpdateFacebookUsername(request));
        }

        /// <summary>
        /// Members have one MemberNewId for each ClientId. Cr/Max/MME
        /// This endpoint will take a MemberNewId for a user return the MemberNewId for that user that matches the supplied ClientId
        /// eg; Max (ClientId: 1000034) MemberNewId to Cr (ClientId: 1000000) MemberNewId
        /// </summary>
        /// <param name="sourceMemberNewId">Known MemberNewId of a user</param>
        /// <param name="targetClientId">Desired MemberNewId matching this ClientId</param>
        /// <returns>MemberNewId for the Member with the supplied MemberNewId that matches the supplied ClientId</returns>
        [HttpGet]
        [Route("member/member-new-id/{sourceMemberNewId}/client-id/{targetClientId:int}/member-new-id")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<IActionResult> GetMemberNewId(string sourceMemberNewId, int targetClientId)
        {
            var targetMemberNewId = await _memberService.MapMembersMemberNewIdToMemberNewIdWithClientId(sourceMemberNewId, targetClientId);
            return targetMemberNewId == null ? (IActionResult)NotFound(null) : Ok(targetMemberNewId);
        }
        /// <summary>
        /// Gets a member by email. If not found a 404 response
        /// is returned.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        /// <response code="500">Internal Server Error</response>

        [HttpGet]
        [Route("member/{email}")]
        [ProducesResponseType(typeof(MemberDto), 200)]
        public async Task<IActionResult> GetMemberByEmail(string email)
        {
            var member = await _memberService.GetMemberByEmail(email);
            return member == null ? (IActionResult)NotFound($"email {email} does not exist as a member") : Ok(member);
        }

    }
}