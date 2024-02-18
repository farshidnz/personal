using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.MemberClick.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Exceptions;
using Newtonsoft.Json;

namespace Cashrewards3API.Features.MemberClick
{
    [Authorize]
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    public class MemberClickController: ControllerBase
    {
        private readonly ILogger<MemberClickController> _logger;
        private readonly IPremiumService _premiumService;
        private readonly IMemberClickService _memberClickService;
        private readonly IMemberClickHistoryService _memberClickHistoryService;
        private readonly IRequestContext _requestContext;
        private const string Notifier = "Notifier";
        
        public MemberClickController(IMemberClickService memberClickService,
                                     IMemberClickHistoryService memberClickHistoryService,
                                     IRequestContext reqestContext,
                                     ILogger<MemberClickController> logger,
                                     IPremiumService _premiumService)
        {
            _memberClickService = memberClickService;
            _memberClickHistoryService = memberClickHistoryService;
            _logger = logger;
            this._premiumService = _premiumService;
            _requestContext = reqestContext;
        }


        [HttpPost]
        [Route("memberclick")]
        [ProducesResponseType(typeof(IEnumerable<TrackingLinkResultModel>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<TrackingLinkResultModel>> GeneratTrackingLink(MemberClickRequestModel request)
        {
            try
            {
                if (_requestContext.Member == null)
                    throw new BadRequestException("Invalid data provied");

                int clientId = _requestContext.HasBearerToken
                    ? await _requestContext.GetClientIdFromDynamoDbAsync()
                    : Constants.Clients.CashRewards;
                
                PremiumMembership premiumMembership = await _premiumService.GetPremiumMembership(clientId, _requestContext.CognitoUserId);

                var premiumClientId = premiumMembership?.IsCurrentlyActive ?? false ? premiumMembership?.PremiumClientId : null;
                var trackingLinkInfo = new TrackingLinkInfoModel
                {
                    CampaignId = request.CampaignId,
                    HyphenatedStringWithType = request.Hyphenated,
                    IpAddress = _requestContext.RemoteIpAddress,
                    UserAgent = _requestContext.IsFromNotifier ? (_requestContext.UserAgent.Contains(Notifier) ? _requestContext.UserAgent : $"{Notifier}/{_requestContext.UserAgent}") : _requestContext.UserAgent,
                    IsMobileApp = request.IsMobile,
                    ClientId = clientId,
                    Member = _requestContext.Member,
                    PremiumClientId = premiumClientId,
                    IncludeTiers = request.IncludeTiers
                };

                
                var trackingLinkData = await _memberClickService.GetMemberClickWithTrackingUrlAsync(trackingLinkInfo);
                _logger.LogInformation($"Generating tracking link for member {_requestContext.Member}. Tracking link data: {JsonConvert.SerializeObject(trackingLinkData)}");
                return Ok(trackingLinkData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode((int)HttpStatusCode.BadRequest);
            }

        }

        [HttpGet]
        [Route("memberclick/{hyphenatedStringWithType}")]
        [ProducesResponseType(typeof(MemberClickTypeDetailsResultModel), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<MemberClickTypeDetailsResultModel>> GetMemberClickHyphenatedStringTypeDetails(string hyphenatedStringWithType)
        {
                int clientId = _requestContext.HasBearerToken
                       ? await _requestContext.GetClientIdFromDynamoDbAsync()
                       : Constants.Clients.CashRewards;

                return Ok(await _memberClickService.GetMemberClickTypeDetails(hyphenatedStringWithType, clientId));
        }

        [HttpGet]
        [Route("memberclicks")]
        [ProducesResponseType(typeof(PagedList<MemberClickHistoryResultModel>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedList<MemberClickHistoryResultModel>>> GetMemberClicksHistory(int offset = 0, int limit = 40)
        {
            int memberId = await _requestContext.GetMemberidFromDynamodbasync();
            if (memberId <= 0)
                throw new BadRequestException("Invalid data provided");

            var model = new MemberClickHistoryRequestInfoModel
            {
                MemberId = memberId,
                Offset = offset,
                Limit = limit
            };

            return Ok(await _memberClickHistoryService.GetMemberClicksHistory(model));
        }
    }
}
