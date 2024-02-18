using Cashrewards3API.Features.Promotion.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using IRequestContext = Cashrewards3API.Common.IRequestContext;

namespace Cashrewards3API.Features.Promotion
{
    public class PromotionController : BaseController
    {
        private readonly IRequestContext _requestContext;
        private readonly IPromotionCacheService _promotionService;
        private readonly IMemberBonusService _memberBonusService;

        public PromotionController(
            IRequestContext requestContext,
            IPromotionCacheService promotionService,
            IMemberBonusService memberBonusService)
        {
            _requestContext = requestContext;
            _promotionService = promotionService;
            _memberBonusService = memberBonusService;
        }

        [HttpGet("promotions/{name}")]
        public async Task<ActionResult<PromotionDto>> GetPromotion([FromRoute] string name)
        {
            (int clientId, int? premiumClientId) = _requestContext.ClientIds;
            var promotion = await _promotionService.GetPromotion(clientId, premiumClientId, name);
            return Ok(promotion);

        }

        [HttpGet("promotions/memberbonus")]
        [ProducesResponseType(typeof(MemberBonusDto), 200)]
        [Authorize]
        public async Task<ActionResult<MemberBonusDto>> GetMemberBonus()
        {
            var accessCode = _requestContext.Member.AccessCode;
            (int clientId, int? premiumClientId) = _requestContext.ClientIds;

            var welcomeBonus = await _memberBonusService.GetMemberBonus(clientId, accessCode);

            return welcomeBonus;
        }
    }
}
