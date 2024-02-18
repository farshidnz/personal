using Cashrewards3API.Features.Person.Interface;
using Cashrewards3API.Features.Person.Model;
using Cashrewards3API.Features.Person.Request.UpdatePerson;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.Person
{
    [Route("api/v1")]
    public class PersonController : BaseController
    {
        private readonly IPerson _personService;

        public PersonController(IPerson personService)
        {
            _personService = personService;
        }

        [HttpPatch]
        [Route("person/update")]
        [Authorize(Policy = "CR-ClientCredentials")]
        [Authorize(Policy = "CR-AccessToken")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdatePerson([FromBody] UpdatePersonRequest request)
        {
            if (string.IsNullOrEmpty(UserToken.CognitoId))
                throw new Exceptions.BadRequestException("Cognito Id is empty");
            request.CognitoId = UserToken.CognitoId;
            await _personService.UpdatePerson(Mapper.Map<PersonModel>(request));
            return Ok();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [Authorize(Policy = "InternalPolicy")]
        [HttpGet()]
        [Route("internal/person/{personId:int}/client/{clientId:int}/memberid")]
        [ProducesResponseType(typeof(int?), 200)]
        public async Task<ActionResult<int?>> getMemberIdFromPersonIdAndClientId(
                    int personId, int clientId)
        {
            var response = await _personService.GetMemberIdFromPersonIdAndClientId(personId, clientId);
            return response != null ? Ok(response) : NotFound($"memberId does not exist for personId {personId} and clientId {clientId}");
        }
    }
}