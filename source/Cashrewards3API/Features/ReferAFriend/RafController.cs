using Cashrewards3API.Exceptions;
using Cashrewards3API.Features.ReferAFriend.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.ReferAFriend
{
    public class RafController : BaseController
    {
        private readonly IRafService _rafService;

        public RafController(IRafService rafService)
        {
            _rafService = rafService;
        }

        [HttpGet("bonus/{newMemberId}")]
        public async Task<ActionResult<RafResultModel>> GetMemberRAFBonus(Guid newMemberId)
        {
            var result = (await _rafService.GetRafBonus(newMemberId.ToString()));
            if (result == null)
            {
                throw new NotFoundException($"No bonus information found for member {newMemberId} ");
            }

            return Ok(result);
        }
    }
}
