using System.Collections.Generic;
using System.Threading.Tasks;
using Cashrewards3API.Common;
using Cashrewards3API.Features.ShopGoClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cashrewards3API.Features.ShopGoClient
{
    [Authorize(Policy = "InternalPolicy")]
    [Route("api/v1/internal")]
    [Produces("application/json")]
    public class ShopGoClientController : ControllerBase
    {
        private readonly IShopGoClientService shopGoClientService;
        #region Constructor(s)        
        public ShopGoClientController(IShopGoClientService shopGoClientService)
        {
            this.shopGoClientService = shopGoClientService;
        }

        #endregion
        /// <summary>
        /// Get all active shopgo clients
        /// </summary>
        /// <returns>Get matter settings.</returns>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpGet]
        [Route("shopgo-clients")]
        [ProducesResponseType(typeof(IEnumerable<ShopGoClientResultModel>), 200)]
        public async Task<ActionResult<ShopGoClientResultModel>> GetClients()
        {
            return Ok(await shopGoClientService.GetShopGoClients());
        }

        [HttpGet]
        [Route("shopgo-clients-all")]
        [ProducesResponseType(typeof(IEnumerable<ShopGoClientResultModel>), 200)]
        public async Task<ActionResult<ShopGoClientResultModel>> GetAllClients()
        {
            return Ok(await shopGoClientService.GetAllShopGoClients());
        }
    }
}