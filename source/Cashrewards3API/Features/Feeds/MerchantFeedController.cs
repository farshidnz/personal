using Cashrewards3API.Common;
using Cashrewards3API.Features.Feeds.Models;
using Cashrewards3API.Features.Feeds.Service;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Feeds
{
    [Route("api/v1/feeds")]
    [ApiController]
    public class MerchantFeedController : Controller
    {
        private readonly IRequestContext _requestContext;
        private readonly IMerchantFeedService _merchantFeedService;

        public MerchantFeedController(
            IRequestContext requestContext,
            IMerchantFeedService merchantFeedService)
        {
            _requestContext = requestContext;
            _merchantFeedService = merchantFeedService;
        }

        [HttpGet]
        [Route("merchants")]
        [ProducesResponseType(typeof(IEnumerable<MerchantFeedModel>), 200)]
        public async Task<IEnumerable<MerchantFeedModel>> GetMerchantFeed()
        {
            (int clientId, int? premiumClientId) = _requestContext.ClientIdsWithoutUserContext;

            return await _merchantFeedService.GetMerchantFeed(clientId, premiumClientId);
        }
    }
}
