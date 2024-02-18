using System.Collections.Generic;
using System.Threading.Tasks;
using Cashrewards3API.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cashrewards3API.Features.Category
{
    [Authorize(Policy = "InternalPolicy")]
    [Route("api/v1/internal/categories")]
    [ApiController]
    public class CategoryInternalController : ControllerBase
    {
        #region Constructor(s)

        private readonly ICategoryService _svc;
        private readonly ILogger<CategoryController> _logger;

        public CategoryInternalController(ICategoryService svc, ILogger<CategoryController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        #endregion

        /// <summary>
        /// Get all categories for internal access only.
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
            _logger.LogInformation("Getting RootCategorySearch for internal");
            var resp = await _svc.GetNonRootCategoriesAsync(status, CategoryTypeEnum.Merchant);
            return Ok(resp);
        }

    }
}
