using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.Merchant;
using Cashrewards3API.Features.Merchant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features
{
    [Authorize(Policy = "InternalPolicy")]
    [Route("api/v1/internal/cms-tracking")]
    [ApiController]
    public class CmsTrackingMerchantController : Controller
    {
        private readonly IMerchantService _merchantService;

        public CmsTrackingMerchantController(IMerchantService merchantService)
        {
            _merchantService = merchantService;
        }
        /// <summary>
        /// Get paged list of merchants, regarless of merchant tiers.
        /// </summary>
        /// <param name="requestModel">A query filter class generated from the query parameters
        /// of the search url
        /// </param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Route("merchants")]
        [ProducesResponseType(typeof(PagedList<CmsTrackingMerchantSearchResonseModel>), 200)]
        public async Task<ActionResult<PagedList<CmsTrackingMerchantSearchResonseModel>>> GetMerchantList([FromQuery] CmsTrackingMerchantSearchFilterRequestModel requestModel)
        {
            var response = await _merchantService.GetCmsTrackingMerchantsSearchByFilter(requestModel);
            return Ok(response);
        }
    }
}
