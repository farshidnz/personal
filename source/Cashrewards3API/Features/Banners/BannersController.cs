using Cashrewards3API.Common;
using Cashrewards3API.Features.Banners.Interface;
using Cashrewards3API.Features.Banners.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Banners
{
    public class BannersController : BaseController
    {
        private readonly IBanner _bannerService;
        private readonly IRequestContext _requestContext;

        public BannersController(
            IBanner bannerService,
            IRequestContext requestContext)
        {
            _bannerService = bannerService;
            _requestContext = requestContext;
        }

        [HttpGet]
        [Route("banners")]
        public async Task<IList<BannerDto>> GetBanner()
        {
            int clientId = _requestContext.HasBearerToken
                ? await _requestContext.GetClientIdFromDynamoDbAsync()
                : Constants.Clients.CashRewards;

            var banners = await _bannerService.GetBannersFromClientId(clientId);

            return Mapper.Map<IEnumerable<Banner>, IList<BannerDto>>(banners);
        }
    }
}
