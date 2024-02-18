using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cashrewards3API.Features.CardLinkedMerchant
{
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    public class CardLinkedMerchantController : ControllerBase
    {
        private readonly IRequestContext _requestContext;
        private readonly ICardLinkedMerchantService _svc;
        private readonly IPremiumService _premiumService;
        private readonly ILogger<CardLinkedMerchantController> _logger;
       

        public CardLinkedMerchantController(
                IRequestContext requestContext,
                ICardLinkedMerchantService svc,
                IPremiumService premiumService,
                ILogger<CardLinkedMerchantController> logger)
        {
            _requestContext = requestContext;
            _svc = svc;
            _premiumService = premiumService;
            _logger = logger;
        }

        /// <summary>
        /// Get all linked cars for mercahants
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Route("merchants/instore")]
        [Route("merchants/linked-cards")]
        [ProducesResponseType(typeof(IEnumerable<CardLinkedMerchantDto>), 200)]
        public async Task<ActionResult<IEnumerable<CardLinkedMerchantDto>>> CardLinkedMerchantsSearch(string filter = null)
        {
            var categoryId = 0;
            if (!string.IsNullOrEmpty(filter))
            {
                categoryId = ParseFilterString(filter).CategoryId;
            }

            int clientId = _requestContext.HasBearerToken
                ? await _requestContext.GetClientIdFromDynamoDbAsync()
                : Constants.Clients.CashRewards;
            var premiumMembership = await _premiumService.GetPremiumMembership(clientId, _requestContext.CognitoUserId);
            var premiumClientId = premiumMembership?.IsCurrentlyActive ?? false ? premiumMembership?.PremiumClientId : null;

            return Ok(await _svc.GetCardLinkedMerchantsAsync(clientId, premiumClientId, categoryId));
        }

        dynamic ParseFilterString(string filterString)
        {
            return FilterParser.Parse(filterString);
        }
    }
}
