using Cashrewards3API.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Category
{
    [ApiController]
    [Route("api/v1/categories")]
    [Produces("application/json")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _svc;
        private readonly IRequestContext _requestContext;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService svc, IRequestContext requestContext, ILogger<CategoryController> logger)
        {
            _svc = svc;
            _requestContext = requestContext;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories.
        /// </summary>
        /// <returns>Get matter settings.</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), 200)]
        public async Task<ActionResult<CategoryDto>> RootCategorySearch(
            [FromQuery] Status status = Status.Active)
        {
            _logger.LogInformation("Getting RootCategorySearch");
            (int clientId, int? premiumClientId) = _requestContext.ClientIdsWithoutUserContext;
            var resp = await _svc.GetRootCategoriesAsync(clientId, premiumClientId, status);
            return Ok(resp);
        }

        /// <summary>
        /// Get all sub categories for a root category ID
        /// </summary>
        /// <returns>Get matter settings.</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Route("{categoryId:int}/sub")]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), 200)]
        public async Task<ActionResult<CategoryDto>> SubCategories(
            int? categoryId,
            [FromQuery] Status status = Status.Active)
        {
            _logger.LogInformation($"Getting sub categories for: {categoryId}");
            if (categoryId == null)
            {
                return BadRequest();
            }

            (int clientId, int? premiumClientId) = _requestContext.ClientIdsWithoutUserContext;
            return Ok(await _svc.GetSubCategoriesAsync(clientId, premiumClientId, categoryId.GetValueOrDefault(), status));
        }
    }
}
