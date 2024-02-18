using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.Merchant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Merchant
{
    /// <summary>
    /// Merchants tier's end point used by promo microservice api
    /// Also now includes some eftpos transaction transformer lambda endpoints
    /// </summary>
    [Authorize(Policy = "InternalPolicy")]
    [Route("api/v1/internal")]
    [ApiController]
    public class MerchantTierInternalController : ControllerBase
    {
        private readonly IMerchantInternalService _merchantInternalService;

        public MerchantTierInternalController(IMerchantInternalService merchantInternalService)
        {
            _merchantInternalService = merchantInternalService;
        }

        /// <summary>
        /// Creates a merchant tier and its corresponding cash rewards merchant tier client
        /// if they do not exist. If they already exist, they will not be created and the
        /// existing merchant tier will be returned.
        /// </summary>
        /// <param name="requestModel"></param>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Route("merchant-tiers")]
        [ProducesResponseType(typeof(MerchantTier), 201)]
        public async Task<ActionResult<MerchantTier>> CreateMerchantTier(CreateInternalMerchantTierRequestModel requestModel)
        {
            var merchantTier = await _merchantInternalService.CreateInternalMerchantTier(requestModel);
            return Created(new Uri($"{Request.GetEncodedUrl()}/{merchantTier.MerchantId}"), merchantTier);
        }

        /// <summary>
        /// Updates a merchant tier and its corresponding cash rewards merchant tier client.
        /// </summary>
        /// <param name="merchantTierId"></param>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("merchant-tiers/{merchantTierId:int}")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> UpdateMerchantTier(int merchantTierId, UpdateInternalMerchantTierRequestModel requestModel)
        {
            requestModel.MerchantTierId = merchantTierId;
            await _merchantInternalService.UpdateInternalMerchantTier(requestModel);
            return NoContent();
        }

        /// <summary>
        /// Updates a merchant tier and its corresponding cash rewards merchant tier client to be inactive.
        /// </summary>
        /// <param name="merchantTierId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("merchant-tiers/{merchantTierId:int}/deactivate")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> DeactivateMerchantTier(int merchantTierId)
        {
            await _merchantInternalService.DeactivateMerchantTier(merchantTierId);
            return NoContent();
        }


        // Functions used by eftpos transaction transformer

        /// <summary>
        /// 
        /// </summary>
        /// <param name="merchantTierId"></param>
        /// <param name="clientId"></param>
        /// <param name="dateTimeUtc"></param>
        /// <returns></returns>
        [HttpGet()]
        [Route("merchant-tier/{merchantTierId:int}/client/{clientId}/date-time/{dateTimeUtc:DateTime}/exists")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<ActionResult<bool>> IsMerchantTierClient(
                    int merchantTierId, int clientId, System.DateTime dateTimeUtc)
        {
            var response = await _merchantInternalService.ExistsMerchantTierClient(merchantTierId, clientId, dateTimeUtc);

            return Ok(response);
        }

        /// <summary>
        /// API for eftpos transformer lambda to access merchant tiers
        /// Return only required data if a merchant tier was found for the merchant at the time given
        /// </summary>
        /// <param name="merchantId">Merchant to check</param>
        /// <param name="dateTimeUtc">UTCDate time to check</param>
        /// <param name="top">Limit returned responses to this many. No order guarentee.</param>
        /// <returns>Important merchant tier columns with some extra merchant and network columns.</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Forbidden Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet()]
        [Route("merchant-tier/merchant/{merchantId:int}/date-time/{dateTimeUtc:DateTime}/active")]
        [ProducesResponseType(typeof(PagedList<MerchantTierEftposTransformer>), 200)]
        public async Task<ActionResult<PagedList<MerchantTierEftposTransformer>>> GetMerchantActiveMerchantTiers(
                    int merchantId, System.DateTime dateTimeUtc, int? top = null)
        {
            var response = await _merchantInternalService.GetActiveMerchantTiers(merchantId, dateTimeUtc, top);

            return Ok(response);
        }


    }
}