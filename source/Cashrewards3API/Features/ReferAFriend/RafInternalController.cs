using Cashrewards3API.Features.ReferAFriend.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.ReferAFriend
{
    [Route("api/v1/internal")]
    [Authorize(Policy = "InternalPolicy")]
    public class RafInternalController : BaseController
    {
        private readonly ITalkableService _talkableService;

        public RafInternalController(ITalkableService talkableService)
        {
            _talkableService = talkableService;
        }

        [HttpPost("talkable/signup")]
        public async Task<ActionResult<TalkableSignupResult>> TalkableSignUp([FromBody] TalkableSignupRequest request)
        {
            var result = await _talkableService.SignUp(request);
            return Ok(result);
        }
    }
}
