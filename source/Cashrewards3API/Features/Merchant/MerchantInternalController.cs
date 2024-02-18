using System.Collections.Generic;
using System.Threading.Tasks;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.Category;
using Cashrewards3API.Features.Merchant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cashrewards3API.Features.Merchant
{

    /// <summary>
    /// Merchant's end point used by promo microservice api
    /// Also now includes some eftpos transaction transformer lambda endpoints
    /// </summary>
    [Authorize(Policy = "InternalPolicy")]
    [Route("api/v1/internal")]
    [ApiController]
    public class MerchantInternalController : ControllerBase
    {
        private readonly IMerchantService _merchantService;
        private readonly ICategoryService _categoryService;

        public MerchantInternalController(
            IMerchantService merchantService, 
            IMerchantInternalService merchantInternalService,
            ICategoryService categoryService)
        {
            _merchantService = merchantService;
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get paged list of merchants
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
        [ProducesResponseType(typeof(PagedList<MerchantSearchResponseModel>), 200)]
        public async Task<ActionResult<PagedList<MerchantSearchResponseModel>>> GetMerchants([FromQuery] MerchantByFilterRequestModel requestModel)
        {
            var response = await _merchantService.GetMerchantsSearchByFilter(requestModel);
            return Ok(response);
        }

        // Functions used by eftpos transaction transformer

        /// <summary>
        /// Get paged list of merchants
        /// </summary>
        /// <param name="id">Merchant Id</param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Route("merchants/{id:int}/categories")]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), 200)]
        public async Task<ActionResult<MerchantBundleBasicModel>> GetCategoriesByMerchantById(
            int id)
        {
            var response = await _categoryService.GetCategoriesByMerchantIdAsync(id);
            return Ok(response);
        }

        /// <summary>
        /// Get paged list of merchants
        /// </summary>
        /// <param name="id">Merchant Id</param>
        /// <returns></returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Route("merchants/{id:int}")]
        [ProducesResponseType(typeof(MerchantSearchResponseModel), 200)]
        public async Task<ActionResult<MerchantSearchResponseModel>> GetMerchantById(
            int id)
        {
            int clientId = Constants.Clients.CashRewards;
            var response = await _merchantService.GetMerchantsSearchById(id, clientId);
            return Ok(response);
        }

        
    }
}
