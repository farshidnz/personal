using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Enum;
using Cashrewards3API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Offers
{

    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    public class OfferController : ControllerBase
    {
        private readonly IRequestContext _requestContext;
        private readonly IOfferService _svc;
        private readonly ILogger<OfferController> _logger;

        #region Constructor

        public OfferController(IRequestContext requestContext,
                               IOfferService svc,
                               IPremiumService premiumService,
                               ILogger<OfferController> logger)
        {
            _requestContext = requestContext;
            _svc = svc;
            _logger = logger;
        }

        #endregion Constructor

        /// <summary>
        /// Get featured offers for clientId
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [Authorize]
        [HttpGet]
        [Route("featured-offers/mobile")]
        [ProducesResponseType(typeof(PagedList<OfferDto>), 200)]
        public async Task<ActionResult<OfferDto>> GetFeaturedOffersForMobile([FromQuery] string filter = null,
                                                                    [FromQuery] int offset = 0,
                                                                    [FromQuery] int limit = 150)
        {
            int clientId = await _requestContext.GetClientIdFromDynamoDbAsync();
            var categoryId = 0;
            if (!string.IsNullOrEmpty(filter))
            {
                categoryId = ParseFilterString(filter).CategoryId;
            }

            return Ok(await _svc.GetFeaturedOffersForMobileAsync(clientId, categoryId, offset, limit));
        }

        /// <summary>
        /// Get featured offers for clientId
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Authorize(Policy = Constants.PolicyNames.AllowAnonymousOrToken)]
        [Route("offer/featured-offers")]
        [ProducesResponseType(typeof(PagedList<OfferDto>), 200)]
        public async Task<ActionResult<IPaginationModel<OfferDto>>> GetFeaturedOffers(
            [FromQuery] string filter = null,
            [FromQuery] int offset = 0,
            [FromQuery] int limit = 150)
        {
            var categoryId = 0;
            if (!string.IsNullOrEmpty(filter))
            {
                categoryId = ParseFilterString(filter).CategoryId;
            }

            (var clientId, var premiumClientId) = _requestContext.ClientIds;

            return Ok(await _svc.GetFeaturedOffersAsync(clientId, premiumClientId, categoryId, offset, limit, _requestContext.IsMobileDevice));
        }

        /// <summary>
        /// Get cashback increased offers for browswer
        /// </summary>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Authorize(Policy = Constants.PolicyNames.AllowAnonymousOrToken)]
        [Route("cashback-increased-offers")]
        [ProducesResponseType(typeof(IEnumerable<OfferDto>), 200)]
        public async Task<ActionResult<OfferDto>> GetCashbackIncreasedOffers()
        {
            (var clientId, var premiumClientId) = _requestContext.ClientIds;

            var offers = _requestContext.IsMobileDevice
                ? await _svc.GetCashBackIncreasedOffersForMobile(clientId, premiumClientId)
                : await _svc.GetCashBackIncreasedOffersForBrowser(clientId, premiumClientId);

            return Ok(offers);
        }

        [HttpGet]
        [Authorize(Policy = Constants.PolicyNames.AllowAnonymousOrToken)]
        [Route("offer/specialoffers")]
        [ProducesResponseType(typeof(IEnumerable<OfferDto>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<SpecialOffersDto>> SpecialOffers([FromQuery] OfferTypeEnum? offerType)
        {
            (int clientId, int? premiumClientId) = _requestContext.ClientIds;
            return Ok(await _svc.GetSpecialOffers(clientId, premiumClientId, offerType, _requestContext.IsMobileDevice));
        }

        /// <summary>
        ///  Get cashback increased offers for mobile
        /// </summary>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [Authorize]
        [HttpGet]
        [Route("cashback-increased-offers/mobile")]
        [ProducesResponseType(typeof(IEnumerable<OfferDto>), 200)]
        public async Task<ActionResult<OfferDto>> GetCashbackIncreasedOffersForMobile()
        {
            int clientId = await _requestContext.GetClientIdFromDynamoDbAsync();
            return Ok(await _svc.GetCashBackIncreasedOffersForMobile(clientId, null));
        }

        private dynamic ParseFilterString(string filterString)
        {
            return FilterParser.Parse(filterString);
        }
    }
}