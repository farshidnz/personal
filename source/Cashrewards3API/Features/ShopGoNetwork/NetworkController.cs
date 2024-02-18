using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.ShopGoNetwork.Model;
using Cashrewards3API.Features.ShopGoNetwork.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.ShopGoNetwork
{
    public class NetowrkController : BaseController
    {
        private readonly INetworkService _networkService;

        public NetowrkController(INetworkService networkService)
        {
            _networkService = networkService;
        }

        [HttpGet]
        [Route("networks")]
        [Authorize(Policy = "InternalPolicy")]
        [ProducesResponseType(typeof(PagedList<NetworkDto>), 200)]
        public async Task<PagedList<NetworkDto>> GetNetworks([FromQuery] NetworkFilterRequest filter)
        {
            return await _networkService.GetNetworks(filter);
        }
    }
}
