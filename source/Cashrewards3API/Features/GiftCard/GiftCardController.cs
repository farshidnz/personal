using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cashrewards3API.Features.GiftCard.Interface;
using Cashrewards3API.Features.GiftCard.Model;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;

namespace Cashrewards3API.Features.GiftCard
{
    public class GiftCardController : BaseController
    {
        private readonly IGiftCard _giftCardService;
        private readonly IRequestContext _requestContext;
        private readonly IPremiumService _premiumService;

        public GiftCardController(IGiftCard giftCardService,
                                    IRequestContext requestContext,
                                    IPremiumService premiumService)
        {
            _giftCardService = giftCardService;
            _requestContext = requestContext;
            _premiumService = premiumService;
        }

        [HttpGet("giftcards/{name}")]
        public async Task<ActionResult<GiftCardDto>> GetGiftCardInfo([FromRoute] string name)
        {
            int clientId = _requestContext.HasBearerToken
                ? await _requestContext.GetClientIdFromDynamoDbAsync()
                : Constants.Clients.CashRewards;
            var premiumMembership = await _premiumService.GetPremiumMembership(clientId, _requestContext.CognitoUserId);
            var premiumClientId = premiumMembership?.IsCurrentlyActive ?? false ? premiumMembership?.PremiumClientId : null;
            var giftCards = await _giftCardService.GetGiftCard(clientId, premiumClientId, name);
            return Ok(giftCards);

        }
    }
}
