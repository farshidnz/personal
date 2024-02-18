using System.Collections.Generic;
using System.Threading.Tasks;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.Category.Models;
using Cashrewards3API.Features.Merchant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cashrewards3API.Features.Category
{

    [ApiController]
    [Route("api/v1/public/categories")]
    [Produces("application/json")]
    public class CategoryPublicController : ControllerBase
    {
        #region Constructor(s)

        private readonly ICategoryService _svc;
        private readonly IMerchantService _merchantService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryPublicController(ICategoryService svc, IMerchantService merchantService, ILogger<CategoryController> logger)
        {
            _svc = svc;
            _merchantService = merchantService;
            _logger = logger;
        }

        #endregion


        /// <summary>
        /// Get all categories including merchant counts.
        /// </summary>
        /// <returns>Get matter settings.</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Route("")]
        [CamelCase]
        [ProducesResponseType(typeof(AllCategoriesModel), 200)]
        public async Task<ActionResult<AllCategoriesModel>> RootCategorySearch(
            [FromQuery] Status status = Status.Active)
        {
            var clientId = Constants.Clients.CashRewards;

            var merchantCount = await _merchantService.GetTotalMerchantCountForClient(clientId);
            var categories = await _svc.GetRootCategoriesWithCountAsync(clientId, null, status);

            var response = new AllCategoriesModel()
            {
                TotalMerchants = merchantCount.TotalMerchantsCount,
                Categories = categories
            };

            return Ok(response);
        }



    }
}